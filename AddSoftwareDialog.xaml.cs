using System.Windows;
using Microsoft.Win32;
using BatchLauncher.Models;

namespace BatchLauncher
{
    public partial class AddSoftwareDialog : Window
    {
        public SoftwareInfo Result { get; private set; } = new SoftwareInfo();

        public AddSoftwareDialog()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "选择程序文件或快捷方式",
                Filter = "可执行文件|*.exe;*.lnk|所有文件|*.*",
                DefaultExt = ".exe"
            };

            if (ofd.ShowDialog() == true)
            {
                txtPath.Text = ofd.FileName;

                // 自动填充名称：去掉扩展名，优先取文件名
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
                    txtName.Text = fileName;
                }
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            var name = txtName.Text.Trim();
            var path = txtPath.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("请输入软件名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("请选择程序路径", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPath.Focus();
                return;
            }

            Result = new SoftwareInfo
            {
                Name = name,
                Path = path,
                Arguments = txtArguments.Text.Trim(),
                IsSelected = false
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}