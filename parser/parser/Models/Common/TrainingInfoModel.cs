using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace parser.Models.Common
{
    public class TrainingInfoModel : INotifyPropertyChanged
    {
        public ObservableCollection<Comment> Comments { get; set; }

        private string commentsFile;
        public string CommentsFile
        {
            get => commentsFile;
            set
            {
                commentsFile = value;
                OnPropertyChanged(nameof(CommentsFile));
            }
        }

        public TrainingInfoModel()
        {
            Comments = new ObservableCollection<Comment>();
            CommentsFile = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
