using Avalonia.Input.TextInput;
using Avalonia.Threading;
using BrickVault;
using BrickVault.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BrickVaultApp.ViewModels
{
    public class MultiProgressWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DATExtractItemViewModel> Items { get; } = new();

        public string Location { get; }

        private string buttonMessage = "Cancel";
        public string ButtonMessage
        {
            get => buttonMessage;
            set
            {
                if (buttonMessage != value)
                {
                    buttonMessage = value;
                    OnPropertyChanged(nameof(ButtonMessage));
                }
            }
        }

        public MultiProgressWindowViewModel(IEnumerable<DATFile> datFiles, string outputLocation)
        {
            foreach (var file in datFiles)
            {
                Items.Add(new DATExtractItemViewModel(file));
            }

            Location = outputLocation;

            StartExtraction();
        }

        private ConcurrentQueue<DATExtractItemViewModel> extractQueue;
        private CancellationTokenSource cancellationTokenSource;
        private int extractedCount = 0;

        private void StartExtraction()
        {
            extractedCount = 0;
            extractQueue = new ConcurrentQueue<DATExtractItemViewModel>(Items);
            int maxThreads = 4;
            cancellationTokenSource = new CancellationTokenSource();

            for (int i = 0; i < maxThreads; i++)
            {
                if (extractQueue.TryDequeue(out var item))
                {
                    ExtractFileAsync(item, cancellationTokenSource);
                }
            }
        }


        private void ExtractFileAsync(DATExtractItemViewModel item, CancellationTokenSource token)
        {
            try
            {
                var ctx = new ThreadedExtractionCtx(1, item.dat.Files.Length, token);

                ctx.OnProgressChange += () =>
                {
                    double progress = (ctx.Extracted / (double)ctx.Total) * 100;
                    
                    Dispatcher.UIThread.Post(() =>
                    {
                        item.Progress = progress;
                        if (item.Progress >= 100)
                        {
                            item.Message = "Complete";
                            if (++extractedCount == Items.Count) // fully extracted
                            {
                                ButtonMessage = "Close";
                            }
                        }   
                    });

                    if (progress >= 100 && extractQueue.TryDequeue(out var newItem))
                    {
                        ExtractFileAsync(newItem, token);
                    }
                };

                item.Message = "Extracting";
                item.dat.ExtractAll(Location, ctx);
            }
            catch (OperationCanceledException)
            {
                ButtonMessage = "Close";
            }
        }

        public bool ButtonPress()
        {
            if (ButtonMessage == "Cancel")
            {
                cancellationTokenSource.Cancel();
                ButtonMessage = "Close";
                return false;
            }
            else
            {
                return true;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
