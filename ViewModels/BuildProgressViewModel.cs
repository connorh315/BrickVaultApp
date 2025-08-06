using BrickVault;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrickVaultApp.ViewModels
{
    public class BuildProgressViewModel : INotifyPropertyChanged
    {
        private string buttonString;
        public string ButtonString
        {
            get => buttonString;
            set => SetField(ref buttonString, value);
        }

        private string progressString;
        public string ProgressString
        {
            get => progressString;
            set => SetField(ref progressString, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }

        private BuildProgress progress;
        public BuildProgress Progress
        {
            get => progress;
            set => SetField(ref progress, value);
        }

        public BuildProgressViewModel(BuildProgress progress)
        {
            Progress = progress;
            progress.PropertyChanged += Progress_PropertyChanged;
        }

        private void Progress_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BuildProgress.Status))
            {
                UpdateProgressString();
                UpdateButtonString();
            }
        }

        private void UpdateProgressString()
        {
            ProgressString = progress.Status switch
            {
                BuildStatus.ScanningFiles => "Scanning Files",
                BuildStatus.PackingFiles => "Packing Files",
                BuildStatus.WritingHeader => "Writing Header",
                BuildStatus.Finalising => "Finalising",
                BuildStatus.Done => "Done",
                BuildStatus.Cancelled => "Cancelled",
                _ => "???"
            };
        }

        private void UpdateButtonString()
        {
            ButtonString = progress.Status switch
            {
                BuildStatus.Done => "Close",
                BuildStatus.Cancelled => "Close",
                _ => "Cancel"
            };
        }

    }
}
