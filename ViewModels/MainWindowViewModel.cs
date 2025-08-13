using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Remote.Protocol;
using BrickVault;
using BrickVault.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrickVaultApp.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TreeNodeViewModel> Nodes { get; } = new ObservableCollection<TreeNodeViewModel>();

        public ObservableCollection<TreeNodeViewModel> SearchResults { get; } = new ObservableCollection<TreeNodeViewModel>();

        public List<TreeNodeViewModel> Leaves = new List<TreeNodeViewModel>();

        public bool HasSearchResults => !string.IsNullOrWhiteSpace(SearchText);

        private DATFile currentDatFile;
        private List<DATFile> currentDatFiles;

        public bool IsFileOpened => currentDatFile != null || (currentDatFiles != null && currentDatFiles.Count > 0);
        public int OpenFilesCount
        {
            get
            {
                if (currentDatFile != null) return 1;
                if (currentDatFiles != null) return currentDatFiles.Count;
                return 0;
            }
        }

        private string searchBoxText = "";
        public string SearchText
        {
            get => searchBoxText;
            set
            {
                if (searchBoxText != value)
                {
                    string previousFilter = searchBoxText;

                    searchBoxText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSearchResults)));

                    FilterTreeView(previousFilter);
                }
            }
        }

        private string extractSelectionHeader = "Extract _selection";
        public string ExtractSelectionHeader
        {
            get => extractSelectionHeader;
            set
            {
                if (extractSelectionHeader != value)
                {
                    extractSelectionHeader = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExtractSelectionHeader)));
                }
            }
        }


        private string extractAllFilesHeader = "Extract _all files";
        public string ExtractAllFilesHeader
        {
            get => extractAllFilesHeader;
            set
            {
                if (extractAllFilesHeader != value)
                {
                    extractAllFilesHeader = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExtractAllFilesHeader)));
                }
            }
        }

        private bool extractToLocation = false;
        public bool ExtractToLocation
        {
            get => extractToLocation;
            set
            {
                extractToLocation = value;
                ExtractSelectionHeader = value ? "Extract _selection to..." : "Extract _selection";
                ExtractAllFilesHeader = value ? "Extract _all files to..." : "Extract _all files";
            }
        }

        private BuildArchiveViewModel buildArchive;
        public BuildArchiveViewModel BuildArchive
        {
            get => buildArchive;
            set
            {
                buildArchive = value;
                IsBuildLoaded = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildArchive)));
            }
        }

        public bool IsBuildLoaded
        {
            get => buildArchive != null;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBuildLoaded)));
            }
        }

        public void FilterTreeView(string previousFilter) // Could do a lot better with this really
        {
            if (currentDatFile == null && currentDatFiles == null) return;

            SearchResults.Clear();

            if (!HasSearchResults)
                return;

            string search = searchBoxText.ToLower();

            bool requiresPaths = search.Contains('\\');

            foreach (var node in Leaves)
            {
                if (requiresPaths)
                {
                    if (node.Path.Contains(search))
                    {
                        SearchResults.Add(node);
                    }
                }
                else
                {
                    if (node.Title.Contains(search))
                    {
                        SearchResults.Add(node);
                    }
                }
            }

            //foreach (var node in Flatten(Nodes))
            //{
            //    if (node.Children.Count == 0 && node.Path.Contains(search))
            //    {
            //        SearchResults.Add(node);
            //    }
            //}
        }

        private IEnumerable<TreeNodeViewModel> Flatten(IEnumerable<TreeNodeViewModel> nodes)
        {
            foreach (var node in nodes)
            {
                yield return node;
                foreach (var child in Flatten(node.Children.Values))
                {
                    yield return child;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel()
        {
            var message = new TreeNodeViewModel("Open a .DAT file to get started", null);
            message.IsEnabled = false;
            Nodes.Add(message);
        }

        public void Reset()
        {
            currentDatFile = null;
            currentDatFiles = null;
            Nodes.Clear();
            Leaves.Clear();
            SearchText = "";

            GC.Collect(); // Required when switching between sets of files as the memory builds up too quickly
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void OpenDatFile(string datFileLocation)
        {
            Reset();

            var dat = DATFile.Open(datFileLocation);

            if (dat == null) return;
            
            TreeNodeViewModel root = new TreeNodeViewModel("root", null);
            foreach (var file in dat.Files)
            {
                var current = root;
                var parts = file.Path.TrimStart('\\').Split('\\');

                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];

                    if (!current.Children.ContainsKey(part))
                    {
                        current.Children[part] = new TreeNodeViewModel(part, current);

                        if (i == parts.Length - 1) // Last part, so it's a file
                        {
                            Leaves.Add(current.Children[part]);
                            current.Children[part].Size = $"[{file.GetFormattedSize()}]";
                            current.Children[part].Archive = "";
                        }
                    }
                    current = current.Children[part];
                }
            }

            foreach (var kvp in root.Children.OrderBy(kv => kv.Key))
            {
                var child = kvp.Value;
                child.Prepare();
                Nodes.Add(child);
            }

            currentDatFile = dat;
        }

        private void AddChildren(TreeNodeViewModel parentVM, FileTreeNode parentNode, DATFile archive)
        {
            foreach (var child in archive.FileTree.EnumerateChildren(parentNode))
            {
                if (!parentVM.Children.ContainsKey(child.Segment))
                {
                    TreeNodeViewModel newNode = new TreeNodeViewModel(child.Segment, parentVM);
                    parentVM.Children[child.Segment] = newNode;

                    if (!child.HasChildren)
                    {
                        Leaves.Add(newNode);
                        newNode.Size = $"[{child.File.GetFormattedSize()}]";
                        newNode.Archive = archive.FileName;
                    }
                }
                var nextChild = parentVM.Children[child.Segment];

                if (child.HasChildren)
                {
                    AddChildren(nextChild, child, archive);
                }
            }
        }

        public void OpenFolder(string folderLocation)
        {
            Reset();

            List<DATFile> openedFiles = new List<DATFile>();

            foreach (var folderFile in Directory.GetFiles(folderLocation, "", AppSettings.Settings.OpenRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                if (!folderFile.ToLower().Contains(".dat")) continue;

                if (DATFile.Open(folderFile) is not DATFile validDat)
                    continue;

                openedFiles.Add(validDat);
            }

            StageTimer.StartStage("Build tree");

            TreeNodeViewModel root = new TreeNodeViewModel("root", null);

            foreach (var dat in openedFiles)
            {
                if (dat.FileTree != null)
                {
                    var rootNode = dat.FileTree.Root;

                    AddChildren(root, rootNode, dat);
                }
                else
                {
                    foreach (var file in dat.Files)
                    {
                        var current = root;
                        var parts = file.Path.TrimStart('\\').Split('\\');

                        for (int i = 0; i < parts.Length; i++)
                        {
                            string part = parts[i];

                            if (!current.Children.ContainsKey(part))
                            {
                                current.Children[part] = new TreeNodeViewModel(part, current);

                                if (i == parts.Length - 1) // Last part, so it's a file
                                {
                                    Leaves.Add(current.Children[part]);
                                    current.Children[part].Size = $"[{file.GetFormattedSize()}]";
                                    current.Children[part].Archive = dat.FileName;
                                }
                            }
                            current = current.Children[part];
                        }
                    }
                }
            }

            StageTimer.StartStage("Build viewmodels");

            foreach (var kvp in root.Children.OrderBy(kv => kv.Key))
            {
                var child = kvp.Value;
                child.Prepare();
                Nodes.Add(child);
            }

            StageTimer.StopStage();

            currentDatFiles = openedFiles;
        }

        public async Task ExtractSelection(List<TreeNodeViewModel> selected, string outputLocation, Window window)
        {
            //List<ArchiveFile> files = new List<ArchiveFile>();
            
            Dictionary<DATFile, List<ArchiveFile>> dict = new();

            if (currentDatFile != null)
                currentDatFiles = new List<DATFile> { currentDatFile };

            foreach (var dat in currentDatFiles)
            {
                dict.Add(dat, new List<ArchiveFile>());
            }
            
            int totalFiles = 0;

            var patch = new List<DATFile>();
            var patchstream = new List<DATFile>();
            var stream = new List<DATFile>();
            var game = new List<DATFile>();

            foreach (var dat in currentDatFiles)
            {
                var name = dat.FileName.ToUpper();
                if (name.Contains("PATCHSTREAM"))
                    patchstream.Add(dat);
                else if (name.Contains("PATCH"))
                    patch.Add(dat);
                else if (name.Contains("STREAM"))
                    stream.Add(dat);
                else
                    game.Add(dat);
            }

            var reordered = new List<DATFile>(patch.Count + stream.Count + game.Count);
            reordered.AddRange(patch);
            reordered.AddRange(patchstream);
            reordered.AddRange(stream);
            reordered.AddRange(game);

            foreach (var selection in selected)
            {
                string path = "\\" + selection.Path;

                if (selection.Children.Count > 0)
                { // selection is folder
                    path += "\\";

                    Dictionary<string, bool> handledFiles = new(); // not my finest piece of work

                    foreach (var dat in reordered)
                    {
                        foreach (ArchiveFile file in dat.Files)
                        {
                            if (file.Path.StartsWith(path))
                            {
                                if (handledFiles.ContainsKey(file.Path)) continue;

                                dict[dat].Add(file);
                                handledFiles[file.Path] = true;
                                totalFiles++;
                                continue;
                            }
                        }
                    }
                }
                else
                { // selection is file
                    bool justBreak = false;
                    foreach (var dat in reordered)
                    {
                        if (justBreak) break;
                        foreach (ArchiveFile file in dat.Files)
                        {
                            if (file.Path == path)
                            {
                                dict[dat].Add(file);
                                justBreak = true;
                                totalFiles++;
                                break;
                            }
                        }
                    }
                }
            }

            CancellationTokenSource source = new CancellationTokenSource();

            ThreadedExtractionCtx threadedCtx = new ThreadedExtractionCtx(1, totalFiles, source);
            threadedCtx.DisplayOutput = AppSettings.Settings.ShouldLogProgressToCommandLine;
            var progressWindow = new ProgressWindow(threadedCtx);

            if (outputLocation == "")
                outputLocation = Path.GetDirectoryName(currentDatFiles[0].FileLocation);

            foreach (var dat in currentDatFiles)
            {
                if (dict[dat].Count > 0)
                {
                    dat.ExtractFiles(dict[dat].ToArray(), outputLocation, threadedCtx);
                }
            }

            await progressWindow.ShowDialog(window);
        }

        public async Task ExtractAll(string outputLocation, Window window)
        {
            if (currentDatFile != null)
            {
                CancellationTokenSource source = new CancellationTokenSource();

                ThreadedExtractionCtx threadedCtx = new ThreadedExtractionCtx(1, currentDatFile.Files.Length, source);
                threadedCtx.DisplayOutput = AppSettings.Settings.ShouldLogProgressToCommandLine;
                var progressWindow = new ProgressWindow(threadedCtx);

                if (outputLocation == "")
                    outputLocation = Path.GetDirectoryName(currentDatFile.FileLocation);

                currentDatFile.ExtractAll(outputLocation, threadedCtx);

                await progressWindow.ShowDialog(window);
            }
            else if (currentDatFiles != null)
            {
                if (outputLocation == "")
                    outputLocation = Path.GetDirectoryName(currentDatFiles[0].FileLocation);

                var multiProgressWindow = new MultiProgressWindow(currentDatFiles, outputLocation);

                await multiProgressWindow.ShowDialog(window);
            }
        }

        public async void Build(Window window)
        {
            if (!IsBuildLoaded) return;

            Reset();

            DATBuildSettings build = new DATBuildSettings();

            build.OutputFileLocation = BuildArchive.ArchivePath;
            build.InputFolderLocation = BuildArchive.ArchiveFolder;
            build.Version = BuildArchive.ArchiveVersion;
            build.ShouldCreateHDR = BuildArchive.BuildHDRFile;
            build.BuilderID = AppSettings.AppString;
            if (BuildArchive.IsMod)
            {
                build.SetupAsMod(BuildArchive.ModAuthor, BuildArchive.ModName, BuildArchive.ModVersion);
            }

            // Setup progress tracking
            var progress = new BuildProgress();
            var modal = new BuildProgressWindow(progress);

            // Start build task
            var buildTask = Task.Run(() =>
            {
                try
                {
                    DATFile.BuildFromFolder(build, progress);
                }
                catch (OperationCanceledException)
                {
                    progress.Status = BuildStatus.Cancelled;
                }
            }, progress.CancellationToken);

            // Show the modal while building
            await modal.ShowDialog(window);

            if (!buildTask.IsCompleted) // User has probably shut the window down manually
            {
                progress.Cancel(); // Triggers cancellation token
            }

            // Optionally open the file after building
            if (progress.Status == BuildStatus.Done)
            {
                BuildArchive.BuildCount++;
                BuildArchive.BuildDate = DateTime.Now;
                BuildList.Update();

                OpenDatFile(build.OutputFileLocation);
            }
        }

        public async Task OpenSettings(AppSettings settings, Window window)
        {
            var settingsWindow = new SettingsWindow(settings);

            await settingsWindow.ShowDialog(window);
        }

        public async Task OpenAbout(Window window)
        {
            var aboutWindow = new AboutWindow();

            await aboutWindow.ShowDialog(window);
        }

        public async Task OpenBuild(Window window)
        {
            var buildWindow = new BuildWindow();

            await buildWindow.ShowDialog(window);

            if (buildWindow.SelectedBuild != null)
            {
                BuildArchive = buildWindow.SelectedBuild;

                Build(window);
            }
        }
    }
}
