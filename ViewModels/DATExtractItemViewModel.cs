using BrickVault.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickVaultApp.ViewModels
{
    public class DATExtractItemViewModel : INotifyPropertyChanged
    {
        public DATFile dat { get; }

        public string FileName { get; }

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

        private string message = "Waiting...";
        public string Message
        {
            get => message;
            set
            {
                if (message != value)
                {
                    message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }

        public DATExtractItemViewModel(DATFile datFile)
        {
            dat = datFile;
            FileName = Path.GetFileName(dat.FileLocation);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
