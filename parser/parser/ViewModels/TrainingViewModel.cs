using parser.Helpers;
using parser.Models;
using parser.Models.Common;
using parser.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace parser.ViewModels
{
    public class TrainingViewModel : AnalyzeCommentsViewModel
    {
        private TrainingInfoModel trainingInfo;
        public TrainingInfoModel TrainingInfo
        {
            get => trainingInfo;
            set
            {
                trainingInfo = value;
                OnPropertyChanged(nameof(TrainingInfo));
            }
        }

        public ObservableCollection<string> StopWords { get; set; }
        public string StopWordsString
        {
            get => string.Join("\n", StopWords);
        }

        public ICommand OpenCommentsFileCommand { get; }
        public ICommand ToLowerCaseCommand { get; }
        public ICommand RemoveSymbolsCommand { get; }
        public ICommand RemoveStopWordsCommand { get; }
        public ICommand ViewStopWordsCommand { get; }

        public TrainingViewModel(TrainingInfoModel _trainingInfo)
        {
            TrainingInfo = _trainingInfo;
            OpenCommentsFileCommand = new Command(OpenCommentsFile);
            ToLowerCaseCommand = new Command(ToLowerCase);
            RemoveSymbolsCommand = new Command(RemoveSymbols);
            RemoveStopWordsCommand = new Command(RemoveStopWords);
            ViewStopWordsCommand = new Command(ViewStopWords);

            StopWords = new ObservableCollection<string>();
            LoadStopWords("ukrainian-stopwords.txt");
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

        private void ToLowerCase(object parameter)
        {
            foreach (Comment comment in TrainingInfo.Comments)
            {
                comment.CommentText = comment.CommentText.ToLower();
            }
        }

        private void RemoveSymbols(object paremter)
        {
            foreach (Comment comment in TrainingInfo.Comments)
            {
                comment.CommentText = Regex.Replace(Regex.Replace(comment.CommentText, @"[^\w\s]", " "), @"[ ]{2,}", " ").Trim();
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

        private void OpenCommentsFile(object parameter)
        {
            TrainingInfo.Comments.Clear();
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "*.tsv|*.csv";
            if (ofd.ShowDialog() ?? true)
            {
                TrainingInfo.CommentsFile = ofd.FileName;
                var separator = Path.GetExtension(TrainingInfo.CommentsFile) == ".csv" ? ',' : '\t';
                using (StreamReader reader = new StreamReader(File.Open(TrainingInfo.CommentsFile, FileMode.Open)))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(separator);
                        int.TryParse(values[1], out var sentiment);
                        TrainingInfo.Comments.Add(new Comment(0, "", values[0], DateTime.Now, sentiment));
                    }
                }
            }
        }
    }
}
