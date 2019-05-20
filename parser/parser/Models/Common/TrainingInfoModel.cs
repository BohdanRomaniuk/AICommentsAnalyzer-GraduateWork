using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace parser.Models.Common
{
    public class TrainingInfoModel : INotifyPropertyChanged
    {
        public ObservableCollection<Comment> Comments { get; set; }

        private string commentsFile;
        public string CommentsFile
        {
            get => Path.GetFileName(commentsFile);
            set
            {
                commentsFile = value;
                if (File.Exists(value))
                {
                    LoadComments(value);
                }
                OnPropertyChanged(nameof(CommentsFile));
            }
        }

        public TrainingInfoModel()
        {
            Comments = new ObservableCollection<Comment>();
            CommentsFile = string.Empty;
        }

        private void LoadComments(string fileName)
        {
            Comments.Clear();
            using (Stream reader = File.Open(fileName, FileMode.Open))
            {
                BinaryFormatter ser = new BinaryFormatter();
                Comments = (ObservableCollection<Comment>)ser.Deserialize(reader);
                OnPropertyChanged(nameof(Comments));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
