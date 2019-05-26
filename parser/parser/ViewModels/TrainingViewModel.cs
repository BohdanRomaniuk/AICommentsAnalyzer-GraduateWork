﻿using parser.Helpers;
using parser.Models;
using parser.Models.Common;
using parser.Windows;
using System;
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

        public ObservableCollection<string> StopWords { get; set; }

        public ICommand OpenCommentsFileCommand { get; }
        public ICommand CleanCommand { get; }
        public ICommand RemoveStopWordsCommand { get; }
        public ICommand ViewStopWordsCommand { get; }
        public ICommand GenerateTrainFileCommand { get; }

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
            //To lower case & removing symbols
            Maximum = CommonInfo.TrainComments.Count;
            Progress = 0;
            foreach (Comment comment in CommonInfo.TrainComments)
            {
                comment.CommentText = Regex.Replace(comment.CommentText.ToLower(), @"[^\w\s]", " ");
                comment.CommentText = Regex.Replace(comment.CommentText, @"[ ]{2,}", " ");
                comment.CommentText = Regex.Replace(comment.CommentText, @"[\d-]", string.Empty).Trim();
                ++Progress;
            }
        }

        private void RemoveStopWords(object parameter)
        {
            foreach (Comment comment in CommonInfo.TrainComments)
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
            if (CommonInfo.TrainComments.Count > 0)
            {
                Directory.CreateDirectory("train");
                int version = 1;
                while(File.Exists(Path.Combine(BaseDirectory, $"train\\comments_{version}.tsv")))
                {
                    ++version;
                }
                TrainFileLocation = $"train\\comments_{version}.tsv";

                using (StreamWriter fileStr = new StreamWriter(new FileStream(Path.Combine(BaseDirectory, TrainFileLocation), FileMode.Create)))
                {
                    ErrorsCount = 0;
                    Progress = 0;
                    var separator = '\t';
                    await fileStr.WriteLineAsync($"id{separator}sentiment{separator}review");
                    foreach (var comment in CommonInfo.TrainComments)
                    {
                        await fileStr.WriteLineAsync($"\"{comment.Id}\"{separator}\"{comment.Sentiment}\"{separator}\"{comment.CommentText}\"");
                        ++Progress;
                    }
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
    }
}
