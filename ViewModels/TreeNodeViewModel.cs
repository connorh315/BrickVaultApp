using Avalonia.Controls.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrickVaultApp.ViewModels
{
    public class TreeNodeViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Path { get; set; }

        public Dictionary<string, TreeNodeViewModel> Children { get; set; } = new();

        public ObservableCollection<TreeNodeViewModel> SubNodes { get; } = new ObservableCollection<TreeNodeViewModel>();

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded = value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));

                    if (_isExpanded && HasPlaceholder)
                    {
                        LoadChildren();
                    }
                }
            }
        }

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        private bool isVisible = true;
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
                }
            }
        }

        public bool HasPlaceholder => SubNodes.Count == 1 && SubNodes[0] is PlaceholderViewModel;

        public TreeNodeViewModel(string title, string path)
        {
            Title = title;
            Path = path;
        }

        private void LoadChildren()
        {
            SubNodes.Clear();
            foreach (var kvp in Children.OrderBy(kv => kv.Key))
            {
                var node = kvp.Value;
                node.Prepare();
                SubNodes.Add(node);
            }
        }

        public void Prepare()
        {
            if (Children.Count > 0)
                SubNodes.Add(new PlaceholderViewModel());
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class PlaceholderViewModel : TreeNodeViewModel
    {
        public PlaceholderViewModel() : base("Loading...", "") { }
    }
}
