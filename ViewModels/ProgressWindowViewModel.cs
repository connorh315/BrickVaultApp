using Avalonia.Threading;
using BrickVault;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BrickVaultApp.ViewModels
{
    public class ProgressWindowViewModel : INotifyPropertyChanged
    {
        private readonly ThreadedExtractionCtx ctx;
        private readonly Stopwatch sw = new();
        private readonly DispatcherTimer timer;

        public ProgressWindowViewModel(ThreadedExtractionCtx ctx)
        {
            this.ctx = ctx;

            sw.Start();

            ctx.OnProgressChange += UpdateProgress;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            timer.Tick += (_, _) =>
            {
                ElapsedTimeText = $"Elapsed: {(int)sw.Elapsed.TotalSeconds}s";
            };

            timer.Start();
        }

        public void OnWindowClose()
        {
            if (ctx.Extracted == ctx.Total)
            {
                return;
            }

            ctx.Cancel.Cancel();
        }

        private void UpdateProgress()
        {
            Progress = (ctx.Extracted / (double)ctx.Total) * 100;
            ProgressText = $"Extracting {ctx.Extracted} / {ctx.Total}";

            if (ctx.Extracted == ctx.Total)
            {
                InteractText = "Close";
                timer.Stop();
            }
        }

        public bool HasComplete => ctx.Extracted == ctx.Total;

        private double progress;
        public double Progress
        {
            get => progress;
            set
            {
                if (progress != value)
                {
                    progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }

        private string progressText = "";
        public string ProgressText
        {
            get => progressText;
            set
            {
                if (progressText != value)
                {
                    progressText = value;
                    OnPropertyChanged(nameof(ProgressText));
                }
            }
        }

        private string elapsedTimeText = "Elapsed: 0s";
        public string ElapsedTimeText
        {
            get => elapsedTimeText;
            set
            {
                if (elapsedTimeText != value)
                {
                    elapsedTimeText = value;
                    OnPropertyChanged(nameof(ElapsedTimeText));
                }
            }
        }

        private string interactText = "Cancel";
        public string InteractText
        {
            get => interactText;
            set
            {
                if (interactText != value)
                {
                    interactText = value;
                    OnPropertyChanged(nameof(InteractText));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
