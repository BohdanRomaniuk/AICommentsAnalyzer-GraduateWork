using parser.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace parser.ViewModels
{
    public class ManualTestViewModel : BaseViewModel
    {
        private string commentText;
        private string clearedText;
        private double percentsOfPositive;
        private string commentSentiment;
        private Brush commentColor;

        public string CommentText
        {
            get => commentText;
            set
            {
                commentText = value;
                OnPropertyChanged(nameof(CommentText));
            }
        }

        public string ClearedText
        {
            get => clearedText;
            set
            {
                clearedText = value;
                OnPropertyChanged(nameof(ClearedText));
            }
        }

        public double PercentsOfPositive
        {
            get => percentsOfPositive;
            set
            {
                percentsOfPositive = value;
                OnPropertyChanged(nameof(PercentsOfPositive));
            }
        }

        public string CommentSentiment
        {
            get => commentSentiment;
            set
            {
                commentSentiment = value;
                OnPropertyChanged(nameof(CommentSentiment));
            }
        }

        public Brush CommentColor
        {
            get => commentColor;
            set
            {
                commentColor = value;
                OnPropertyChanged(nameof(CommentColor));
            }
        }

        public ObservableCollection<string> StopWords { get; set; }
        public ICommand StartTestCommand { get; }

        public ManualTestViewModel()
        {
            StartTestCommand = new Command(StartTest);

            StopWords = new ObservableCollection<string>();
            LoadStopWords("ukrainian-stopwords.txt");
        }

        public async void StartTest(object parameter)
        {
            if (!string.IsNullOrEmpty(CommentText))
            {
                try
                {
                    //Cleaning
                    //Remove symbols
                    ClearedText = (string)CommentText.Clone();
                    ClearedText = Regex.Replace(ClearedText.ToLower(), @"[^\w\s]", " ");
                    //Remove digits
                    ClearedText = Regex.Replace(ClearedText, @"[\d-]", string.Empty);
                    //Remove non cyrilic symbols
                    ClearedText = Regex.Replace(ClearedText, @"[a-zA-z]+", string.Empty);
                    //Remove spaces
                    ClearedText = Regex.Replace(ClearedText, @"[ ]{2,}", " ").Trim();

                    //Stopwords
                    var words = ClearedText.Split(' ').ToList();
                    for (int i = 0; i < words.Count; ++i)
                    {
                        if (StopWords.Contains(words[i]))
                        {
                            words.Remove(words[i]);
                            --i;
                        }
                    }
                    ClearedText = string.Join(" ", words);

                    await Task.Run(() =>
                    {
                        ProcessStartInfo start = new ProcessStartInfo();
                        start.FileName = "C:/Users/Bohdan/AppData/Local/Programs/Python/Python36/python.exe";
                        var cmd = $"\"{Path.Combine(BaseDirectory, "ai\\test_manual.py")}\"";
                        var args = ClearedText;
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

                        var percents = Regex.Match(result, "[0-9]+.[0-9]+");
                        if (!percents.Success)
                        {
                            throw new Exception("Нейронна мережа повернула невідомий результат!");
                        }

                        PercentsOfPositive = Convert.ToDouble(percents.Value.Replace('.', ','));
                        if (PercentsOfPositive > 0.5)
                        {
                            CommentSentiment = "Позитивний";
                            CommentColor = Brushes.Green;
                        }
                        else
                        {
                            CommentSentiment = "Негативний";
                            CommentColor = Brushes.Red;
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
