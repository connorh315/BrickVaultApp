using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using BrickVault;
using BrickVault.Types;
using BrickVaultApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Avalonia.Input;
using MsBox.Avalonia;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BrickVaultApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
            Title = AppSettings.AppString;
        }

        private void MainWindow_KeyUp(object? sender, KeyEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            if (e.Key == Key.LeftShift)
            {
                vm.ExtractToLocation = false;
            }
        }

        private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            if (e.Key == Key.LeftShift)
            {
                vm.ExtractToLocation = true;
            }
        }

        private void OpenDatFile(string path)
        {
            MainWindowViewModel vm = (MainWindowViewModel)DataContext!;
            vm?.OpenDatFile(path);
            Title = $"{AppSettings.AppString} | Viewing DAT file: {path}";
        }

        private async void MenuItem_Open_Click(object? sender, RoutedEventArgs e)
        {
            if (StorageProvider == null)
                throw new Exception("Unable to access filesystem");

            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open DAT File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("DAT files") { Patterns = new[] { "*.DAT", "*.DAT2", "*.DATWIN", "*.DATNX" } }
                }
            });

            if (files.Count > 0)
            {
                string filePath = files[0].Path.LocalPath;
                OpenDatFile(filePath);
            }
        }

        private async Task<string?> CreateFolderModal(string title)
        {
            if (StorageProvider == null)
                throw new Exception("Unable to access filesystem");

            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
            });

            if (folder.Count == 0) return null;

            return folder[0].TryGetLocalPath() ?? folder[0].Path.OriginalString;
        }

        private async void MenuItem_Open_Folder_Click(object? sender, RoutedEventArgs e)
        {
            string? selectedPath = await CreateFolderModal("Select Game Folder");
            if (selectedPath == null) return;

            MainWindowViewModel vm = (MainWindowViewModel)DataContext!;
            vm.OpenFolder(selectedPath);

            Title = $"{AppSettings.AppString} | Viewing {vm.OpenFilesCount} files in {selectedPath}";
        }

        private void Window_DragDrop(object sender, DragEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            var firstFile = e.Data.GetFiles()?.FirstOrDefault();
            if (firstFile == null)
                return;

            OpenDatFile(firstFile.Path.LocalPath);
        }
        
        private void MenuItem_Exit_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void MenuItem_ExtractAll_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            if (!vm.IsFileOpened) return;

            string? outputLocation = "";
            if (vm.ExtractToLocation)
            {
                outputLocation = await CreateFolderModal("Choose output location");
                if (outputLocation == null) return;
            }

            await vm.ExtractAll(outputLocation, this);
        }

        private async void MenuItem_ExtractSelection_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            if (!vm.IsFileOpened) return;

            List<TreeNodeViewModel> selection;

            if (vm.HasSearchResults)
                selection = FileList.SelectedItems.OfType<TreeNodeViewModel>().ToList();
            else if (FileGrid.SelectedItem is TreeNodeViewModel sel)
                selection = new List<TreeNodeViewModel>() { sel };
            else
                return;

            string? outputLocation = "";
            if (vm.ExtractToLocation)
            {
                outputLocation = await CreateFolderModal("Choose output location");
                if (outputLocation == null) return;
            }

            await vm.ExtractSelection(selection, outputLocation, this);
        }

        private async void MenuItem_BuildArchive_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            await vm.OpenBuild(this);
        }

        private async void MenuItem_Help(object? sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = AppSettings.SupportPage,
                UseShellExecute = true
            });
        }

        private async void MenuItem_About(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            await vm.OpenAbout(this);
        }

        private async void MenuItem_Settings(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            await vm.OpenSettings(AppSettings.Settings, this);
        }

        private async void MenuItem_Rebuild_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
                return;

            vm.Build(this);
        }

        private void SearchResult_DoubleTapped(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is TreeNodeViewModel selection && DataContext is MainWindowViewModel vm)
            {
                string[] nodeSegments = selection.Path.Split("\\");

                vm.SearchText = ""; // This must be here, as Avalonia will not generate the containers unless the control is being rendered

                ExpandAndSelectNode(FileGrid, nodeSegments, 0, selection);
            }
        }

        private void ExpandAndSelectNode(ItemsControl parent, string[] segments, int segmentCounter, TreeNodeViewModel target)
        {
            foreach (var item in parent.Items)
            {
                var test = parent.ContainerFromItem(item);
                if (item is TreeNodeViewModel vm && parent.ContainerFromItem(item) is TreeViewItem tv)
                {
                    if (item == target)
                    {
                        tv.IsSelected = true;
                        tv.BringIntoView();
                        return;
                    }
                    else if (vm.Title == segments[segmentCounter])
                    {
                        vm.IsExpanded = true;
                        Dispatcher.UIThread.Post(() =>
                        { // Needed to wait for Avalonia to populate the TreeViewItem
                            ExpandAndSelectNode(tv, segments, segmentCounter + 1, target); 
                        }, DispatcherPriority.Background);

                        return;
                    }
                }
            }
        }
    }
}