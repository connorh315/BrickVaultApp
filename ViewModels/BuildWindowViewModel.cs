using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrickVaultApp.ViewModels
{
    public class BuildWindowViewModel
    {
        public ObservableCollection<BuildArchiveViewModel> BuildArchives { get; set; }

        public void AddToBuildList(BuildArchiveViewModel buildVM)
        {
            BuildArchives.Add(buildVM);

            // Reorder collection by BuildDate descending
            var sorted = BuildArchives.OrderByDescending(b => b.BuildDate).ToList();

            BuildArchives.Clear();
            foreach (var item in sorted)
            {
                BuildArchives.Add(item);
            }

            BuildList.Serialize(BuildArchives);
        }

        public void RemoveFromBuildList(BuildArchiveViewModel buildVM)
        {
            BuildArchives.Remove(buildVM);

            BuildList.Serialize(BuildArchives);
        }

        public BuildWindowViewModel()
        {
            BuildArchives = BuildList.Deserialize();
        }

        public async Task AddToBuildSettings(Window window)
        {
            BuildArchiveViewModel buildVM = new BuildArchiveViewModel();

            var buildSettingsWindow = new BuildSettingsWindow(buildVM);

            await buildSettingsWindow.ShowDialog(window);

            if (buildVM.CommitSettings)
            {
                buildVM.CommitSettings = false;
                buildVM.AlreadyExists = true;

                buildVM.BuildDate = DateTime.Now;

                AddToBuildList(buildVM);
            }
        }

        public async Task OpenBuildSettings(Window window, BuildArchiveViewModel buildVM)
        {
            var duplicate = new BuildArchiveViewModel();
            duplicate.CopyFrom(buildVM);

            var buildSettingsWindow = new BuildSettingsWindow(duplicate);

            await buildSettingsWindow.ShowDialog(window);

            if (duplicate.ShouldDelete)
            {
                RemoveFromBuildList(buildVM);
            }

            if (duplicate.CommitSettings)
            {
                buildVM.CopyFrom(duplicate);

                BuildList.Serialize(BuildArchives);
            }
        }
    }
}
