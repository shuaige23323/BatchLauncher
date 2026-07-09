using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BatchLauncher.Models;

namespace BatchLauncher.Services
{
    public static class ConfigService
    {
        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BatchLauncher");

        private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static List<SoftwareInfo> Load()
        {
            try
            {
                if (!File.Exists(ConfigFile))
                    return new List<SoftwareInfo>();

                var json = File.ReadAllText(ConfigFile);
                var list = JsonSerializer.Deserialize<List<SoftwareInfo>>(json, JsonOptions);
                return list ?? new List<SoftwareInfo>();
            }
            catch
            {
                return new List<SoftwareInfo>();
            }
        }

        public static void Save(IEnumerable<SoftwareInfo> list)
        {
            try
            {
                Directory.CreateDirectory(ConfigDir);
                var json = JsonSerializer.Serialize(list, JsonOptions);
                File.WriteAllText(ConfigFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BatchLauncher] 配置保存失败: {ex.Message}");
            }
        }
    }
}