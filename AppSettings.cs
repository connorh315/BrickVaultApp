using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickVaultApp
{
    public class AppSettings : INotifyPropertyChanged
    {
        public const string AppName = "BrickVault";
        public const string Version = "1.0.0";

        public static string AppString => $"{AppName} {Version}";

        public static AppSettings Settings = Load();

        private const string settingsFile = "settings.txt";

        public const string SupportPage = "https://github.com/connorh315/BrickVault/Support/GettingStarted.md";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool shouldLogProgressToCommandLine = false;
        public bool ShouldLogProgressToCommandLine
        {
            get => shouldLogProgressToCommandLine;
            set
            {
                if (shouldLogProgressToCommandLine == value) return;
                shouldLogProgressToCommandLine = value;
                OnPropertyChanged(nameof(ShouldLogProgressToCommandLine));
            }
        }

        private bool openRecursively = false;
        public bool OpenRecursively
        {
            get => openRecursively;
            set
            {
                if (openRecursively == value) return;
                openRecursively = value;
                OnPropertyChanged(nameof(OpenRecursively));
            }
        }

        public void Save()
        {
            var lines = new List<string>();

            foreach (var prop in typeof(AppSettings).GetProperties())
            {
                if (prop.CanRead)
                {
                    var value = prop.GetValue(this)?.ToString() ?? string.Empty;
                    lines.Add($"{prop.Name}={value}");
                }
            }

            File.WriteAllLines(settingsFile, lines);
        }

        public static AppSettings Load()
        {
            AppSettings settings = new AppSettings();

            if (File.Exists(settingsFile))
            {
                string[] lines = File.ReadAllLines(settingsFile);
                foreach (string line in lines)
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length != 2) continue;

                    string name = parts[0].Trim();
                    string value = parts[1].Trim();

                    var prop = typeof(AppSettings).GetProperty(name);
                    if (prop != null && prop.CanWrite)
                    {
                        try
                        {
                            var convertedValue = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromString(value);
                            prop.SetValue(settings, convertedValue);
                        }
                        catch
                        {
                            // Ignore or log invalid setting
                        }
                    }
                }
            }

            return settings;
        }
    }
}
