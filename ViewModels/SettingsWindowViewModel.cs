using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickVaultApp.ViewModels
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public AppSettings Settings { get; }

        public SettingsWindowViewModel(AppSettings settings)
        {
            this.Settings = settings;
        }

        public void SaveSettings()
        {
            Settings.Save();
        }
    }
}
