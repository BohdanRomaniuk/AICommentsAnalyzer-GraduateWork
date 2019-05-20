using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace parser.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;

        //ProgressBar
        #region progress bar
        private int progress;
        private int maximum;
        private int errorsCount;
        public int Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        public int Maximum
        {
            get => maximum;
            set
            {
                maximum = value;
                OnPropertyChanged(nameof(Maximum));
            }
        }
        public int ErrorsCount
        {
            get => errorsCount;
            set
            {
                errorsCount = value;
                OnPropertyChanged(nameof(ErrorsCount));
            }
        }
        #endregion progress bar

        public BaseViewModel()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
