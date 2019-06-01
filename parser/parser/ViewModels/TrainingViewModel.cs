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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace parser.ViewModels
{
    public class TrainingViewModel : AnalyzeCommentsViewModel
    {
        private CommonInfoModel trainingInfo;
        private string trainFileLocation;
        private int uniqueWordsCount;
        private int meanWordsLength;
        private int maxWordLength;

        public CommonInfoModel CommonInfo
        {
            get => trainingInfo;
            set
            {
                trainingInfo = value;
                OnPropertyChanged(nameof(CommonInfo));
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

        public int UniqueWordsCount
        {
            get => uniqueWordsCount;
            set
            {
                uniqueWordsCount = value;
                OnPropertyChanged(nameof(UniqueWordsCount));
            }
        }

        public int MeanWordsLength
        {
            get => meanWordsLength;
            set
            {
                meanWordsLength = value;
                OnPropertyChanged(nameof(MeanWordsLength));
            }
        }

        public int MaxWordLength
        {
            get => maxWordLength;
            set
            {
                maxWordLength = value;
                OnPropertyChanged(nameof(MaxWordLength));
            }
        }

        public ObservableCollection<string> StopWords { get; set; }

        public ICommand OpenCommentsFileCommand { get; }
        public ICommand CleanCommand { get; }
        public ICommand RemoveStopWordsCommand { get; }
        public ICommand ViewStopWordsCommand { get; }
        public ICommand GenerateTrainFileCommand { get; }
        public ICommand RemoveTextOverflowCommand { get; }

        public ICommand StartTrainCommand { get; }

        public TrainingViewModel(CommonInfoModel _commonInfo)
        {
            CommonInfo = _commonInfo;
            OpenCommentsFileCommand = new Command(OpenCommentsFile);
            CleanCommand = new Command(Clean);
            RemoveStopWordsCommand = new Command(RemoveStopWords);
            ViewStopWordsCommand = new Command(ViewStopWords);

            StopWords = new ObservableCollection<string>();
            LoadStopWords("ukrainian-stopwords.txt");

            GenerateTrainFileCommand = new Command(GenerateTrainFile);
            StartTrainCommand = new Command(StartTrain);
            DeleteCommentCommand = new Command(DeleteComment);
            RemoveTextOverflowCommand = new Command(RemoveTextOverflow);
            MaxWordLength = 130;
        }

        private void StartTrain(object parameter)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "C:/Users/Bohdan/AppData/Local/Programs/Python/Python36/python.exe";
            var cmd = $"\"{Path.Combine(BaseDirectory, "ai\\train.py")}\"";
            var args = $"\"{Path.Combine(BaseDirectory, TrainFileLocation)}\"";
            start.Arguments = string.Format("{0} {1}", cmd, args);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.CreateNoWindow = false;
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
            Maximum = CommonInfo.TrainComments.Count;
            Progress = 0;
            foreach (var comment in CommonInfo.TrainComments)
            {
                //Remove symbols
                comment.CommentText = Regex.Replace(comment.CommentText.ToLower(), @"[^\w\s]", " ");
                //Remove digits
                comment.CommentText = Regex.Replace(comment.CommentText, @"[\d-]", string.Empty);
                //Remove non cyrilic symbols
                //comment.CommentText = Regex.Replace(comment.CommentText, @"[a-zA-z]+", string.Empty);
                //Remove spaces
                comment.CommentText = Regex.Replace(comment.CommentText, @"[ ]{2,}", " ").Trim();
                ++Progress;
            }
            CalculateWordsCount();
        }

        private void RemoveStopWords(object parameter)
        {
            foreach (var comment in CommonInfo.TrainComments)
            {
                var words = comment.CommentText.Split(' ').ToList();
                for (int i = 0; i < words.Count; ++i)
                {
                    if (StopWords.Contains(words[i]))
                    {
                        words.Remove(words[i]);
                        --i;
                    }
                }
                comment.CommentText = string.Join(" ", words);
            }
            CalculateWordsCount();
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
            if (CommonInfo.TrainComments.Count > 0)
            {
                Directory.CreateDirectory("train");
                int version = 1;
                while (File.Exists(Path.Combine(BaseDirectory, $"train\\comments_{version}.tsv")))
                {
                    ++version;
                }
                TrainFileLocation = $"train\\comments_{version}.tsv";

                using (StreamWriter fileStr = new StreamWriter(new FileStream(Path.Combine(BaseDirectory, TrainFileLocation), FileMode.Create)))
                {
                    ErrorsCount = 0;
                    Progress = 0;
                    var separator = '\t';
                    int count = 0;
                    await fileStr.WriteLineAsync($"id{separator}sentiment{separator}review");
                    foreach (var comment in CommonInfo.TrainComments)
                    {
                        if (!string.IsNullOrEmpty(comment.CommentText))
                        {
                            ++count;
                            await fileStr.WriteLineAsync($"\"{comment.Id}\"{separator}\"{comment.Sentiment}\"{separator}\"{comment.CommentText}\"");
                        }
                        ++Progress;
                    }
                    MessageBox.Show($"Збережено {count} коментарів, {CommonInfo.TrainComments.Count - count} вилучено", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void OpenCommentsFile(object parameter)
        {
            CommonInfo.TrainComments.Clear();
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "bin(*.bin)|*.bin";
            if (ofd.ShowDialog() ?? true)
            {
                CommonInfo.TrainFile = ofd.FileName;
            }
        }

        private void DeleteComment(object parameter)
        {
            if (!CommonInfo.TrainComments.Remove(parameter as Comment))
            {
                MessageBox.Show("Неможливо видалити коментар", "Помилка видалення", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateWordsCount()
        {
            Dictionary<string, int> uniqueWords = new Dictionary<string, int>();
            foreach (var comment in CommonInfo.TrainComments)
            {
                var words = comment.CommentText.Split(' ');
                foreach (var word in words)
                {
                    uniqueWords[word] = uniqueWords.TryGetValue(word, out int value) ? ++value : 1;
                }
            }
            UniqueWordsCount = uniqueWords.Count;
            MeanWordsLength = uniqueWords.Sum(c => c.Value) / CommonInfo.TrainComments.Count;
        }

        private void RemoveTextOverflow(object parameter)
        {
            foreach (var comment in CommonInfo.TrainComments)
            {
                var words = comment.CommentText.Split(' ');
                if (words.Count() > MaxWordLength)
                {
                    var result = words.Take(MaxWordLength);
                    comment.CommentText = string.Join(" ", result);
                }
            }
            CalculateWordsCount();
        }
    }
}
