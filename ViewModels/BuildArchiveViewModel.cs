using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static BrickVault.Types.DATFile;

namespace BrickVaultApp.ViewModels
{
    public class BuildArchiveViewModel : INotifyPropertyChanged
    {
        public BuildArchiveViewModel() { AlreadyExists = false; }

        public BuildArchiveViewModel(string friendlyName, string archiveFolder, string archivePath, DateTime buildDate, uint buildCount)
        {
            AlreadyExists = true;

            FriendlyName = friendlyName;
            ArchiveFolder = archiveFolder;
            ArchivePath = archivePath;
            BuildDate = buildDate;
            BuildCount = buildCount;
        }

        private string archivePath;
        public string ArchivePath
        {
            get => archivePath;
            set
            {
                archivePath = value;
                OnPropertyChanged();
            }
        }

        private string archiveFolder;
        public string ArchiveFolder
        {
            get => archiveFolder;
            set
            {
                archiveFolder = value;
                OnPropertyChanged();
            }
        }

        private string friendlyName;
        public string FriendlyName
        {
            get => friendlyName;
            set
            {
                friendlyName = value;
                OnPropertyChanged();
            }
        }

        private DateTime buildDate;
        public DateTime BuildDate
        {
            get => buildDate;
            set
            {
                buildDate = value;
                LastBuilt = FormatFriendlyDate(BuildDate);
                OnPropertyChanged();
            }
        }

        private string FormatFriendlyDate(DateTime dateTime)
        {
            if (BuildCount == 0) return "Never";

            var now = DateTime.Now;
            var difference = now - dateTime;

            if (difference.TotalSeconds < 60)
                return "Just now";

            if (difference.TotalMinutes < 60)
                return $"{(int)difference.TotalMinutes} minute{(difference.TotalMinutes >= 2 ? "s" : "")} ago";

            if (difference.TotalHours < 24)
                return $"{(int)difference.TotalHours} hour{(difference.TotalHours >= 2 ? "s" : "")} ago";

            if (dateTime.Date == now.Date.AddDays(-1))
                return $"Yesterday at {dateTime:HH:mm}";

            if (difference.TotalDays <= 7)
                return $"{(int)difference.TotalDays} day{(difference.TotalDays >= 2 ? "s" : "")} ago";

            return dateTime.ToString("dd/MM/yyyy HH:mm:ss");
        }


        private string lastBuilt = "Never";
        public string LastBuilt
        {
            get => lastBuilt;
            private set
            {
                lastBuilt = value;
                OnPropertyChanged();
            }
        }

        private uint buildCount;
        public uint BuildCount
        {
            get => buildCount;
            set
            {
                uint original = buildCount;
                buildCount = value;

                if (original == 0)
                {
                    LastBuilt = FormatFriendlyDate(BuildDate);
                }
            }
        }

        private bool buildHDRFile;
        public bool BuildHDRFile
        {
            get => buildHDRFile;
            set
            {
                buildHDRFile = value;
                OnPropertyChanged();
            }
        }

        private DATVersion selectedVersion;
        public DATVersion ArchiveVersion
        {
            get => selectedVersion;
            set
            {
                selectedVersion = value;
                OnPropertyChanged();
            }
        }

        public List<DATVersion> AvailableArchiveTypes { get; } = new()
        {
            DATVersion.V11
        };

        private bool isMod;
        public bool IsMod
        {
            get => isMod;
            set
            {
                isMod = value;
                OnPropertyChanged(nameof(IsMod));
                ArchiveType = ""; // just to trigger the setter onpropertychanged
            }
        }
        public string ArchiveType { 
            get
            {
                return IsMod ? "MOD" : "GAME";
            } 
            set
            {
                OnPropertyChanged(nameof(ArchiveType));
            }
        }

        private string modName;
        public string ModName
        {
            get => modName;
            set
            {
                modName = value;
                OnPropertyChanged();
            }
        }

        private string modAuthor;
        public string ModAuthor
        {
            get => modAuthor;
            set
            {
                modAuthor = value;
                OnPropertyChanged();
            }
        }

        private string modVersion;
        public string ModVersion
        {
            get => modVersion;
            set
            {
                modVersion = value;
                OnPropertyChanged();
            }
        }

        public void SetModInfo(string modName, string modAuthor, string modVersion)
        {
            IsMod = true;
            ModName = modName;
            ModAuthor = modAuthor;
            ModVersion = modVersion;
        }

        public bool ShouldDelete = false;

        public bool AlreadyExists { get; set; }

        public bool CommitSettings = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void CopyFrom(BuildArchiveViewModel original)
        {
            AlreadyExists = original.AlreadyExists;

            this.FriendlyName = original.FriendlyName;
            this.ArchiveFolder = original.ArchiveFolder;
            this.ArchivePath = original.ArchivePath;
            this.BuildDate = original.BuildDate;
            this.BuildCount = original.BuildCount;

            this.BuildHDRFile = original.BuildHDRFile;
            this.ArchiveVersion = original.ArchiveVersion;

            if (original.IsMod)
            {
                SetModInfo(original.ModName, original.ModAuthor, original.ModVersion);
            }
            else
            {
                IsMod = false;
                ModName = "";
                ModAuthor = "";
                ModVersion = "";
            }
        }
    }
}
