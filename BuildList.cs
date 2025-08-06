using BrickVault;
using BrickVaultApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrickVault.Types.DATFile;

namespace BrickVaultApp
{
    public static class BuildList
    {
        public static readonly uint Version = 1;

        private static ObservableCollection<BuildArchiveViewModel> list;

        public static ObservableCollection<BuildArchiveViewModel> Deserialize()
        {
            if (list != null) return list;

            if (!File.Exists("builds.list")) return new ObservableCollection<BuildArchiveViewModel>();
            using (RawFile file = new RawFile("builds.list"))
            {
                uint count = file.ReadUInt(true);
                uint version = file.ReadUInt(true);
                if (version > Version)
                    throw new Exception("builds.list version greater than this tool's version? Have you got a more up-to-date version of BrickVault that you should be using instead? Refusing to open.");

                ObservableCollection<BuildArchiveViewModel> builds = new();

                for (int i = 0; i < count; i++)
                {
                    string archivePath = file.ReadNullString();
                    string archiveFolder = file.ReadNullString();
                    string friendlyName = file.ReadNullString();
                    DateTime buildDate = new DateTime(file.ReadLong(true));
                    uint buildCount = file.ReadUInt(true);
                    bool buildHdrFile = file.ReadByte() == 1 ? true : false;
                    DATVersion archiveVersion = (DATVersion)file.ReadByte();
                    var build = new BuildArchiveViewModel(friendlyName, archiveFolder, archivePath, buildDate, buildCount);

                    build.BuildHDRFile = buildHdrFile;
                    build.ArchiveVersion = archiveVersion;

                    bool isMod = file.ReadByte() == 1 ? true : false;
                    if (isMod)
                    {
                        string modName = file.ReadNullString();
                        string modAuthor = file.ReadNullString();
                        string modVersion = file.ReadNullString();
                        build.SetModInfo(modName, modAuthor, modVersion);
                    }

                    builds.Add(build);
                }

                list = builds;

                return builds;
            }
        }

        public static void Serialize(ObservableCollection<BuildArchiveViewModel> builds)
        {
            list = builds;

            using (RawFile file = RawFile.Create("builds.list"))
            {
                file.WriteUInt((uint)builds.Count, true); 
                file.WriteUInt(Version, true);            

                foreach (var build in builds)
                {
                    file.WriteString(build.ArchivePath, 1);
                    file.WriteString(build.ArchiveFolder, 1);
                    file.WriteString(build.FriendlyName, 1);
                    file.WriteLong(build.BuildDate.Ticks, true);
                    file.WriteUInt(build.BuildCount, true);
                    file.WriteByte((byte)(build.BuildHDRFile ? 1 : 0));
                    file.WriteByte((byte)build.ArchiveVersion);
                    file.WriteByte(build.IsMod ? (byte)1 : (byte)0);

                    if (build.IsMod)
                    {
                        file.WriteString(build.ModName, 1);
                        file.WriteString(build.ModAuthor, 1);
                        file.WriteString(build.ModVersion, 1);
                    }
                }
            }
        }

        public static void Update()
        {
            Serialize(list);
        }
    }
}
