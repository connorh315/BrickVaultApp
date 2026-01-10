using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickVaultApp.Settings
{
    public class OpenWithEntry : INotifyPropertyChanged
    {
        private string extension = "";
        public string Extensions
        {
            get => extension;
            set { extension = value; OnPropertyChanged(nameof(Extensions)); }
        }

        private string applicationPath = "";
        public string ApplicationPath
        {
            get => applicationPath;
            set { applicationPath = value; OnPropertyChanged(nameof(ApplicationPath)); }
        }

        public bool Validate()
        {
            bool valid = true;
            if (string.IsNullOrEmpty(Extensions))
            {
                Extensions = " ";
                Extensions = "";
                valid = false;
            }

            if (string.IsNullOrEmpty(ApplicationPath))
            {
                ApplicationPath = " ";
                ApplicationPath = "";
                valid = false;
            }

            return valid;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
