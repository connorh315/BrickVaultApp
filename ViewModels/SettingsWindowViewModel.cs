using BrickVaultApp.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickVaultApp.ViewModels
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public AppSettings Settings { get; }

        public ICommand RemoveOpenWithCommand { get; }

        public SettingsWindowViewModel(AppSettings settings)
        {
            this.Settings = settings;
            RemoveOpenWithCommand = new DelegateCommand(
                execute: p => Remove((OpenWithEntry)p),
                canExecute: p => p is OpenWithEntry);
        }

        private void Remove(OpenWithEntry entry)
        {
            Settings.OpenWithApps.Remove(entry);
        }

        public void AddNewOpenWithEntry()
        {
            Settings.OpenWithApps.Add(new OpenWithEntry());
        }

        public void SaveSettings()
        {
            Settings.Save();
        }
    }
}
