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
    public class TestViewModel : BaseViewModel
    {
        private CommonInfoModel commonInfo;
        private string testFileLocation;
        private int uniqueWordsCount;
        private int meanWordsLength;
        private int maxWordLength;

        public CommonInfoModel CommonInfo
        {
            get => commonInfo;
            set
            {
                commonInfo = value;
                OnPropertyChanged(nameof(CommonInfo));
            }
        }

        public string TestFileLocation
        {
            get => testFileLocation;
            set
            {
                testFileLocation = value;
                OnPropertyChanged(nameof(TestFileLocation));
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

        public ICommand StartTestCommand { get; }

        public ICommand CleanCommand { get; }
        public ICommand GenerateTestFileCommand { get; }
        public ICommand RemoveStopWordsCommand { get; }
        public ICommand ViewStopWordsCommand { get; }

        public ICommand RemoveTextOverflowCommand { get; }

        public TestViewModel(CommonInfoModel _commonInfo)
        {
            CommonInfo = _commonInfo;
            OpenCommentsFileCommand = new Command(OpenCommentsFile);
            StartTestCommand = new Command(StartTest);

            StopWords = new ObservableCollection<string>();
            LoadStopWords("ukrainian-stopwords.txt");

            CleanCommand = new Command(Clear);
            GenerateTestFileCommand = new Command(GenerateTestFile);
            RemoveStopWordsCommand = new Command(RemoveStopWords);
            ViewStopWordsCommand = new Command(ViewStopWords);
            RemoveTextOverflowCommand = new Command(RemoveTextOverflow);
            MaxWordLength = 130;
        }

        private async void StartTest(object parameter)
        {
            try
            {


                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "C:/Users/Bohdan/AppData/Local/Programs/Python/Python36/python.exe";
                var cmd = $"\"{Path.Combine(BaseDirectory, "ai\\test.py")}\"";
                var args = $"\"{Path.Combine(BaseDirectory, TestFileLocation)}\"";
                start.Arguments = string.Format("{0} {1}", cmd, args);
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.CreateNoWindow = false;
                string result = string.Empty;
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                }
                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("Виникла невідома помилка під час тестування!");
                }
                CommonInfo.TestComments.Clear();
                CommonInfo.LoadTestComments($"parse/{CommonInfo.TestFile}");
                using (var stream = new StreamReader(new FileStream("test/result.tsv", FileMode.Open)))
                {
                    await stream.ReadLineAsync();
                    while (!stream.EndOfStream)
                    {
                        var line = await stream.ReadLineAsync();
                        var lineValues = line.Split('\t');
                        int.TryParse(lineValues[0].Replace("\"", string.Empty), out var id);
                        int.TryParse(lineValues[1].Replace("\"", string.Empty), out var sentiment);
                        var comment = CommonInfo.TestComments.FirstOrDefault(x => x.Id == id);
                        if (comment != null)
                        {
                            comment.Sentiment = sentiment;
                        }
                    }
                }
                MessageBox.Show("Тестування завершено", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GenerateTestFile(object parameter)
        {
            if (CommonInfo.TestComments.Count > 0)
            {
                Directory.CreateDirectory("test");
                int version = 1;
                while (File.Exists(Path.Combine(BaseDirectory, $"train\\comments_{version}.tsv")))
                {
                    ++version;
                }
                TestFileLocation = $"test\\comments_{version}.tsv";

                using (StreamWriter fileStr = new StreamWriter(new FileStream(Path.Combine(BaseDirectory, TestFileLocation), FileMode.Create)))
                {
                    ErrorsCount = 0;
                    Progress = 0;
                    int count = 0;
                    var separator = '\t';
                    await fileStr.WriteLineAsync($"id{separator}review");
                    foreach (var comment in CommonInfo.TestComments)
                    {
                        if (!string.IsNullOrEmpty(comment.CommentText))
                        {
                            await fileStr.WriteLineAsync($"\"{comment.Id}\"{separator}\"{comment.CommentText}\"");
                            ++count;
                        }
                        ++Progress;
                    }
                    MessageBox.Show($"Збережено {count} коментарів, {CommonInfo.TestComments.Count - count} вилучено", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Clear(object parameter)
        {
            Maximum = CommonInfo.TestComments.Count;
            Progress = 0;
            foreach (var comment in CommonInfo.TestComments)
            {
                //Remove symbols
                comment.CommentText = Regex.Replace(comment.CommentText.ToLower(), @"[^\w\s]", " ");
                //Remove digits
                comment.CommentText = Regex.Replace(comment.CommentText, @"[\d-]", string.Empty);
                //Remove non cyrilic symbols
                comment.CommentText = Regex.Replace(comment.CommentText, @"[a-zA-z]+", string.Empty);
                //Remove spaces
                comment.CommentText = Regex.Replace(comment.CommentText, @"[ ]{2,}", " ").Trim();
                ++Progress;
            }
            CalculateWordsCount();
        }

        private void RemoveStopWords(object parameter)
        {
            foreach (var comment in CommonInfo.TestComments)
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
            CommonInfo.TestComments.Clear();
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "bin(*.bin)|*.bin";
            if (ofd.ShowDialog() ?? true)
            {
                CommonInfo.TestFile = ofd.FileName;
            }
        }

        private void CalculateWordsCount()
        {
            Dictionary<string, int> uniqueWords = new Dictionary<string, int>();
            foreach (var comment in CommonInfo.TestComments)
            {
                var words = comment.CommentText.Split(' ');
                foreach (var word in words)
                {
                    uniqueWords[word] = uniqueWords.TryGetValue(word, out int value) ? ++value : 1;
                }
            }
            UniqueWordsCount = uniqueWords.Count;
            MeanWordsLength = uniqueWords.Sum(c => c.Value) / CommonInfo.TestComments.Count;
        }

        private void RemoveTextOverflow(object parameter)
        {
            foreach (var comment in CommonInfo.TestComments)
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
