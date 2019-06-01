using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace parser.Models.Common
{
    public class CommonInfoModel : INotifyPropertyChanged
    {
        public ObservableCollection<Comment> TrainComments { get; set; }
        public ObservableCollection<Comment> TestComments { get; set; }

        private string trainFile;
        private string testFile;

        public string TrainFile
        {
            get => Path.GetFileName(trainFile);
            set
            {
                trainFile = value;
                if (File.Exists(value))
                {
                    LoadTrainComments(value);
                }
                OnPropertyChanged(nameof(TrainFile));
            }
        }

        public string TestFile
        {
            get => Path.GetFileName(testFile);
            set
            {
                testFile = value;
                if (File.Exists(value))
                {
                    LoadTestComments(value);
                }
                OnPropertyChanged(nameof(TestFile));
            }
        }

        public CommonInfoModel()
        {
            TrainComments = new ObservableCollection<Comment>();
            TestComments = new ObservableCollection<Comment>();
            TrainFile = string.Empty;
            TestFile = string.Empty;
        }

        private void LoadTrainComments(string fileName)
        {
            TrainComments.Clear();
            using (Stream reader = File.Open(fileName, FileMode.Open))
            {
                BinaryFormatter ser = new BinaryFormatter();
                TrainComments = (ObservableCollection<Comment>)ser.Deserialize(reader);
                OnPropertyChanged(nameof(TrainComments));
            }
        }

        public void LoadTestComments(string fileName)
        {
            TestComments.Clear();
            using (Stream reader = File.Open(fileName, FileMode.Open))
            {
                BinaryFormatter ser = new BinaryFormatter();
                TestComments = (ObservableCollection<Comment>)ser.Deserialize(reader);
                OnPropertyChanged(nameof(TestComments));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
