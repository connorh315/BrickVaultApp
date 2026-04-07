using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickVaultApp.Settings
{
    public class AppSettings : INotifyPropertyChanged
    {
        public const string AppName = "BrickVault";
        public const string Version = "1.2.0";

        public static string AppString => $"{AppName} {Version}";

        public static AppSettings Settings = Load();

        private const string settingsFile = "settings.txt";

        public const string SupportPage = "https://github.com/connorh315/BrickVault/blob/master/Support/GettingStarted.md";

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

        private bool shouldRememberPaths = false;
        public bool ShouldRememberPaths
        {
            get => shouldRememberPaths;
            set
            {
                if (shouldRememberPaths == value) return;
                shouldRememberPaths = value;
                OnPropertyChanged(nameof(ShouldRememberPaths));
            }
        }

        private bool extractPatchFirst = false;
        public bool ExtractPatchFirst
        {
            get => extractPatchFirst;
            set
            {
                if (extractPatchFirst == value) return;
                extractPatchFirst = value;
                OnPropertyChanged(nameof(ExtractPatchFirst));
            }
        }

        private string lastOpenFolderDirectory = string.Empty;
        public string LastOpenFolderDirectory
        {
            get => lastOpenFolderDirectory;
            set
            {
                if (lastOpenFolderDirectory == value) return;
                lastOpenFolderDirectory = value;
                OnPropertyChanged(nameof(LastOpenFolderDirectory));
                if (shouldRememberPaths)
                    Save();
            }
        }

        private string lastExtractDirectory = string.Empty;
        public string LastExtractDirectory
        {
            get => lastExtractDirectory;
            set
            {
                if (lastExtractDirectory == value) return;
                lastExtractDirectory = value;
                OnPropertyChanged(nameof(LastExtractDirectory));
                if (shouldRememberPaths)
                    Save();
            }
        }

        public ObservableCollection<OpenWithEntry> OpenWithApps { get; } = new();

        public string? GetAppForFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                return null;

            var fileLower = file.ToLowerInvariant();

            foreach (var entry in OpenWithApps)
            {
                if (string.IsNullOrWhiteSpace(entry.Extensions))
                    continue;

                foreach (var ext in entry.Extensions.Split('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    var normalizedExt = Normalize(ext);

                    if (fileLower.EndsWith("." + normalizedExt, StringComparison.OrdinalIgnoreCase))
                        return entry.ApplicationPath;
                }
            }

            return null;
        }

        private static string Normalize(string ext) =>
            ext.Trim().TrimStart('.').ToLowerInvariant();

        public ObservableCollection<string> OpenFolderPaths = new();

        public void Save()
        {
            if (DoNotSave) return;

            var lines = new List<string>();

            foreach (var prop in typeof(AppSettings).GetProperties())
            {
                if (prop.Name == nameof(OpenWithApps) || prop.Name == nameof(OpenFolderPaths))
                    continue;

                if (prop.CanRead)
                {
                    var value = prop.GetValue(this)?.ToString() ?? string.Empty;
                    lines.Add($"{prop.Name}={value}");
                }
            }

            lines.Add("");
            lines.Add("[OpenWith]");

            foreach (var entry in OpenWithApps)
                lines.Add($"{entry.Extensions}={entry.ApplicationPath}");

            lines.Add("");
            lines.Add("[OpenFolderHistory]");
            foreach (var entry in OpenFolderPaths)
                lines.Add($"{entry}");

            File.WriteAllLines(settingsFile, lines);
        }

        private static bool DoNotSave = false;

        public static AppSettings Load()
        {
            DoNotSave = true;

            var settings = new AppSettings();

            if (!File.Exists(settingsFile))
                return settings;

            bool inSection = false;
            string section = string.Empty;

            foreach (var line in File.ReadAllLines(settingsFile))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    inSection = true;
                    section = line[1..^1];
                    continue;
                }

                if (!inSection)
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length != 2) continue;

                    var prop = typeof(AppSettings).GetProperty(parts[0]);
                    if (prop?.CanWrite == true)
                    {
                        try
                        {
                            var value = TypeDescriptor
                                .GetConverter(prop.PropertyType)
                                .ConvertFromString(parts[1]);
                            prop.SetValue(settings, value);
                        }
                        catch { }
                    }
                }
                else if (section == "OpenWith")
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length != 2) continue;

                    settings.OpenWithApps.Add(new OpenWithEntry
                    {
                        Extensions = parts[0],
                        ApplicationPath = parts[1]
                    });
                }
                else if (section == "OpenFolderHistory")
                {
                    settings.OpenFolderPaths.Add(line);
                }
            }

            DoNotSave = false;

            return settings;
        }

    }
}
