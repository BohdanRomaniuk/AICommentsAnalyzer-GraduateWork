﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace parser.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<object> Models { get; set; }

        public MainViewModel()
        {
            Models = new ObservableCollection<object>
            {
                new ParsingViewModel()
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
