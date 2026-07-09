using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BatchLauncher.Models;
using BatchLauncher.Services;

namespace BatchLauncher
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<SoftwareInfo> _softwareList = new();
        private bool _isUpdatingSelectAll;

        public MainWindow()
        {
            InitializeComponent();
            LoadSoftwareList();
            lstSoftware.ItemsSource = _softwareList;
        }

        private void LoadSoftwareList()
        {
            var saved = ConfigService.Load();
            _softwareList = new ObservableCollection<SoftwareInfo>(saved);
            foreach (var item in _softwareList)
                item.PropertyChanged += Software_PropertyChanged;
        }

        private void Software_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SoftwareInfo.IsSelected) && !_isUpdatingSelectAll)
                UpdateSelectAllCheckbox();
        }

        private void UpdateSelectAllCheckbox()
        {
            _isUpdatingSelectAll = true;
            if (_softwareList.Count == 0)
            {
                chkSelectAll.IsChecked = false;
            }
            else
            {
                var allSelected = _softwareList.All(s => s.IsSelected);
                var noneSelected = _softwareList.All(s => !s.IsSelected);
                chkSelectAll.IsChecked = allSelected ? true : (noneSelected ? false : null);
            }
            _isUpdatingSelectAll = false;
        }

        //全选/全不选
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            _isUpdatingSelectAll = true;
            var check = chkSelectAll.IsChecked;
            foreach (var item in _softwareList)
                item.IsSelected = check ?? false;
            _isUpdatingSelectAll = false;
        }

        private void AddSoftware_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddSoftwareDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                _softwareList.Add(dialog.Result);
                dialog.Result.PropertyChanged += Software_PropertyChanged;
                ConfigService.Save(_softwareList);
                UpdateStatus("已添加新软件");
            }
        }

        private async void StartSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = _softwareList.Where(s => s.IsSelected).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("请先勾选要启动的软件", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await LaunchSoftwareList(selected);
        }

        private async void StartAll_Click(object sender, RoutedEventArgs e)
        {
            var all = _softwareList.ToList();
            if (all.Count == 0)
            {
                MessageBox.Show("软件列表为空，请先添加软件", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await LaunchSoftwareList(all);
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = _softwareList.Where(s => s.IsSelected).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("请先勾选要删除的软件", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var item in selected)
            {
                item.PropertyChanged -= Software_PropertyChanged;
                _softwareList.Remove(item);
            }
            ConfigService.Save(_softwareList);
            UpdateStatus($"已删除 {selected.Count} 个软件");
            UpdateSelectAllCheckbox();
        }

        private async Task LaunchSoftwareList(IReadOnlyList<SoftwareInfo> list)
        {
            int success = 0, fail = 0;
            UpdateStatus($"正在依次启动 {list.Count} 个软件...");

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                try
                {
                    if (!File.Exists(item.Path) && !Directory.Exists(item.Path))
                    {
                        MessageBox.Show(
                            $"软件 \"{item.Name}\" 的路径不存在，可能已被删除或移动：\n{item.Path}",
                            "路径无效",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        fail++;
                        UpdateStatus($"[{i + 1}/{list.Count}] \"{item.Name}\" 路径无效，已跳过");
                        continue;
                    }

                    var psi = new ProcessStartInfo
                    {
                        FileName = item.Path,
                        UseShellExecute = true,
                        Arguments = item.Arguments ?? string.Empty
                    };
                    Process.Start(psi);
                    success++;
                    UpdateStatus($"[{i + 1}/{list.Count}] 已启动 \"{item.Name}\"");

                    if (i < list.Count - 1)
                        await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    fail++;
                    UpdateStatus($"[{i + 1}/{list.Count}] 启动 \"{item.Name}\" 失败: {ex.Message}");
                }
            }

            UpdateStatus($"完成 — 成功 {success} 个, 失败 {fail} 个");
        }

        private void UpdateStatus(string message)
        {
            txtStatus.Text = message;
        }
    }
}