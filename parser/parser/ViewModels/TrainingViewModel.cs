using parser.Helpers;
using parser.Models;
using parser.Models.Common;
using parser.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace parser.ViewModels
{
    public class TrainingViewModel : AnalyzeCommentsViewModel
    {
        private TrainingInfoModel trainingInfo;
        private string trainFileLocation;

        public TrainingInfoModel TrainingInfo
        {
            get => trainingInfo;
            set
            {
                trainingInfo = value;
                OnPropertyChanged(nameof(TrainingInfo));
            }
        }

        public string TrainFileLocation
        {
            get => trainFileLocation;
            set
            {
                trainFileLocation = value;
                OnPropertyChanged(nameof(TrainFileLocation));
            }
        }

        public ObservableCollection<string> StopWords { get; set; }

        public ICommand OpenCommentsFileCommand { get; }
        public ICommand CleanCommand { get; }
        public ICommand RemoveStopWordsCommand { get; }
        public ICommand ViewStopWordsCommand { get; }
        public ICommand GenerateTrainFileCommand { get; }

        public ICommand StartTrainCommand { get; }

        public TrainingViewModel(TrainingInfoModel _trainingInfo)
        {
            TrainingInfo = _trainingInfo;
            OpenCommentsFileCommand = new Command(OpenCommentsFile);
            CleanCommand = new Command(Clean);
            RemoveStopWordsCommand = new Command(RemoveStopWords);
            ViewStopWordsCommand = new Command(ViewStopWords);

            StopWords = new ObservableCollection<string>();
            LoadStopWords("ukrainian-stopwords.txt");

            GenerateTrainFileCommand = new Command(GenerateTrainFile);
            StartTrainCommand = new Command(StartTrain);
        }

        private void StartTrain(object parameter)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "C:/Users/Bohdan/AppData/Local/Programs/Python/Python36/python.exe";
            var cmd = "C:/test.py";
            var args = "";
            start.Arguments = string.Format("{0} {1}", cmd, args);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.CreateNoWindow = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    MessageBox.Show(result);
                }
            }
        }



        private void LoadStopWords(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (StreamReader reader = new StreamReader(File.Open(fileName, FileMode.Open)))
                {
                    while (!reader.EndOfStream)
                    {
                        StopWords.Add(reader.ReadLine().Trim());
                    }
                }
            }
        }

        private void Clean(object parameter)
        {
            //To lower case & removing symbols
            Maximum = TrainingInfo.Comments.Count;
            Progress = 0;
            foreach (Comment comment in TrainingInfo.Comments)
            {
                comment.CommentText = Regex.Replace(comment.CommentText.ToLower(), @"[^\w\s]", " ");
                comment.CommentText = Regex.Replace(comment.CommentText, @"[ ]{2,}", " ");
                comment.CommentText = Regex.Replace(comment.CommentText, @"[\d-]", string.Empty).Trim();
                ++Progress;
            }
        }

        private void RemoveStopWords(object parameter)
        {
            foreach (Comment comment in TrainingInfo.Comments)
            {
                var words = comment.CommentText.Split(' ').ToList();
                for (int i = 0; i < words.Count; ++i)
                {
                    if (StopWords.Contains(words[i]))
                    {
                        words.Remove(words[i]);
                    }
                }
                comment.CommentText = string.Join(" ", words);
            }
        }

        private void ViewStopWords(object parameter)
        {
            if (StopWords != null && StopWords.Count != 0)
            {
                PreviewWindow tw = new PreviewWindow(this);
                tw.Show();
                tw.Owner = ((MainWindow)System.Windows.Application.Current.MainWindow);
            }
        }

        private async void GenerateTrainFile(object parameter)
        {
            if (TrainingInfo.Comments.Count > 0)
            {
                Directory.CreateDirectory("train");
                TrainFileLocation = $"train\\comments_{DateTime.Now.ToString().Replace(":", "").Replace(" ", "_")}.csv";
                using (StreamWriter fileStr = new StreamWriter(new FileStream(Path.Combine(BaseDirectory, TrainFileLocation), FileMode.Create)))
                {
                    ErrorsCount = 0;
                    Progress = 0;
                    await fileStr.WriteLineAsync("id,text,sentiment");
                    foreach (var comment in TrainingInfo.Comments)
                    {
                        await fileStr.WriteLineAsync($"{comment.Id},{comment.CommentText},{comment.Sentiment}");
                        ++Progress;
                    }
                }
            }
        }

        private void OpenCommentsFile(object parameter)
        {
            TrainingInfo.Comments.Clear();
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "bin(*.bin)|*.bin";
            if (ofd.ShowDialog() ?? true)
            {
                TrainingInfo.CommentsFile =  ofd.FileName;
            }
        }
    }
}
