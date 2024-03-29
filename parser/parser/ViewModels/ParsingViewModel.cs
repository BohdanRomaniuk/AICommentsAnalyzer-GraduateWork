﻿using HtmlAgilityPack;
using Newtonsoft.Json;
using parser.Helpers;
using parser.Models;
using parser.Models.Common;
using parser.Models.Json;
using parser.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace parser.ViewModels
{
    public class ParsingViewModel : AnalyzeCommentsViewModel
    {
        private bool isMoviesMode;
        private string url;
        private int fromPage;
        private int toPage;
        private bool isSavingMode;

        public bool IsMoviesMode
        {
            get => isMoviesMode;
            set
            {
                isMoviesMode = value;
                OnPropertyChanged(nameof(IsMoviesMode));
                OnPropertyChanged(nameof(IsMoviesListVisible));
                OnPropertyChanged(nameof(IsCommentsListVisible));
            }
        }

        public Visibility IsMoviesListVisible => IsMoviesMode ? Visibility.Visible : Visibility.Hidden;
        public Visibility IsCommentsListVisible => !IsMoviesMode ? Visibility.Visible : Visibility.Hidden;

        public string Url
        {
            get => url;
            set
            {
                url = value;
                OnPropertyChanged(nameof(Url));
            }
        }
        public int FromPage
        {
            get => fromPage;
            set
            {
                fromPage = value;
                OnPropertyChanged(nameof(FromPage));
            }
        }
        public int ToPage
        {
            get => toPage;
            set
            {
                toPage = value;
                OnPropertyChanged(nameof(ToPage));
            }
        }

        //Parsing
        #region parsing
        private string title;
        private int from;
        private int to;
        private int sleepTime;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        public int From
        {
            get => from;
            set
            {
                from = value;
                OnPropertyChanged(nameof(From));
            }
        }
        public int To
        {
            get => to;
            set
            {
                to = value;
                OnPropertyChanged(nameof(To));
            }
        }
        public int SleepTime
        {
            get => sleepTime;
            set
            {
                sleepTime = value;
                OnPropertyChanged(nameof(SleepTime));
            }
        }
        public bool IsSavingMode
        {
            get => isSavingMode;
            set
            {
                isSavingMode = value;
                OnPropertyChanged(nameof(IsSavingMode));
                OnPropertyChanged(nameof(IsSleepTimeVisible));
            }
        }
        public Visibility IsSleepTimeVisible => !IsSavingMode ? Visibility.Visible : Visibility.Collapsed;
        #endregion

        public ObservableCollection<Movie> Movies { get; set; }
        public ObservableCollection<Comment> Comments { get; set; }

        //Common
        private CommonInfoModel trainingInfo;
        public CommonInfoModel CommonInfo
        {
            get => trainingInfo;
            set
            {
                trainingInfo = value;
                OnPropertyChanged(nameof(CommonInfo));
            }
        }

        private bool IsTrainCommentsGenerated => CommonInfo.TrainComments.Count != 0;
        public string TrainCommentsGeneratedTitle => IsTrainCommentsGenerated ? "[Згенерована]" : "[Не згенерована]";
        public Brush TrainCommentsGeneratedColor => IsTrainCommentsGenerated ? Brushes.Green : Brushes.Red;

        private bool IsTestCommentsGenerated => CommonInfo.TrainComments.Count != 0;
        public string TestCommentsGeneratedTitle => IsTestCommentsGenerated ? "[Згенерована]" : "[Не згенерована]";
        public Brush TestCommentsGeneratedColor => IsTestCommentsGenerated ? Brushes.Green : Brushes.Red;

        public ICommand GetAllInfoCommand { get; }
        public ICommand OpenParsingMovieWindowCommand { get; }
        public ICommand StartCommand { get; private set; }
        public ICommand ShowMovieCommand { get; }

        public ICommand OpenFromBinaryCommand { get; }
        public ICommand SaveToBinaryCommand { get; }

        public ICommand OpenSaveTrainCommentsWindowCommand { get; }
        public ICommand OpenSaveTestCommentsWindowCommand { get; }
        public ICommand RemoveNonUkrainianCommand { get; }
        public ICommand OpenCheckSpellingWindowCommand { get; }

        public ICommand LoadResultCommand { get; }

        public ParsingViewModel(CommonInfoModel _commonInfo)
        {
            CommonInfo = _commonInfo;
            IsMoviesMode = true;
            Url = @"https://uafilm.tv/films/";
            FromPage = 1;
            ToPage = 1;
            IsSavingMode = false;
            Movies = new ObservableCollection<Movie>();
            Comments = new ObservableCollection<Comment>();
            GetAllInfoCommand = new Command(GetAllInfo);
            OpenParsingMovieWindowCommand = new Command(OpenParsingMovieWindow);
            ShowMovieCommand = new Command(ShowMovie);
            OpenFromBinaryCommand = new Command(OpenFromBinary);
            SaveToBinaryCommand = new Command(SaveToBinary);
            OpenSaveTrainCommentsWindowCommand = new Command(OpenSaveTrainCommentsWindow);
            OpenSaveTestCommentsWindowCommand = new Command(OpenSaveTestCommentsWindow);
            DeleteCommentCommand = new Command(DeleteComment);
            RemoveNonUkrainianCommand = new Command(RemoveNonUkrainian);
            OpenCheckSpellingWindowCommand = new Command(OpenCheckSpellingWindow);
            LoadResultCommand = new Command(LoadResult);
        }

        private async void GetAllInfo(object parameter)
        {
            //Movies.Clear();
            Progress = 0;
            Maximum = ToPage - FromPage + 1;
            HtmlWeb web = new HtmlWeb();
            for (int page = FromPage; page <= ToPage; ++page)
            {
                HtmlDocument doc = new HtmlDocument();
                await Task.Run(() =>
                {
                    doc = web.Load($"{Url}page/{page}/");
                });
                HtmlNodeCollection urls = doc.DocumentNode.SelectNodes("//a[@class='movie-title']");
                for (int i = 0; i < urls.Count; ++i)
                {
                    try
                    {
                        string name = urls[i].InnerText;
                        string url = urls[i].GetAttributeValue("href", "");
                        int pos = url.LastIndexOf('/');
                        int id = Convert.ToInt32(url.Substring(pos + 1, url.IndexOf('-') - pos - 1));
                        Movie toAdd = new Movie(name, url);
                        toAdd.Id = id;
                        Movies.Add(toAdd);
                    }
                    catch (Exception exc)
                    {
                        ++ErrorsCount;
                        MessageBox.Show(exc.Message + "\n" + (Movies.Count - i) + "\n" + Movies[i].Name + "\n" + Movies[i].Link, "Виникла помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                ++Progress;
            }
        }

        private void OpenParsingMovieWindow(object parameter)
        {
            if (Movies.Count != 0)
            {
                IsSavingMode = false;
                Title = "Парсинг фільмів";
                StartCommand = new Command(StartParsing);
                PagesWindow ppw = new PagesWindow(this);
                ppw.Show();
                ppw.Owner = ((MainWindow)System.Windows.Application.Current.MainWindow); ;
            }
        }

        private async void StartParsing(object parameter)
        {
            ErrorsCount = 0;
            Progress = 0;
            Maximum = To - From + 1;
            int fromIndex = Movies.Count - From;
            int toIndex = Movies.Count - To;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = new HtmlDocument();
            for (int i = fromIndex; i >= toIndex; --i)
            {
                try
                {
                    //Getting movie info
                    await Task.Run(() =>
                    {
                        doc = web.Load(Movies[i].Link);

                        Movies[i].UkrName = doc.DocumentNode.SelectSingleNode("//div[@id='mc-right']/h1")?.InnerText ?? "";
                        Movies[i].OriginalName = doc.DocumentNode.SelectSingleNode("//div[@id='mc-right']/span")?.InnerText ?? "";
                        if (string.IsNullOrEmpty(Movies[i].OriginalName))
                        {
                            Movies[i].OriginalName = Movies[i].UkrName;
                        }

                        var labels = doc.DocumentNode.SelectNodes("//div[@class='mi-label-desc']");
                        var infos = doc.DocumentNode.SelectNodes("//div[@class='mi-desc']");
                        var poster = doc.DocumentNode.SelectSingleNode("//div[@class='m-img']/img")?.Attributes["src"]?.Value ?? "no-poster.jpg";
                        var story = doc.DocumentNode.SelectSingleNode("//div[@class='m-desc full-text clearfix']")?.InnerHtml ?? "<div class=\"m-info\">";
                        var imdb = doc.DocumentNode.SelectSingleNode("//div[@class='mr-item-rate']")?.InnerText?.Trim()?.Replace('.', ',') ?? "0";
                        if (labels.Any(x => x.InnerText == "Добірка:"))
                        {
                            labels.Remove(labels.FirstOrDefault(x => x.InnerText == "Добірка:"));
                        }

                        Movies[i].Director = StringHelper.GetPropertyValueByLabel("Режисер:", labels, infos);
                        Movies[i].Actors = StringHelper.GetPropertyValueByLabel("В ролях:", labels, infos);
                        Movies[i].Companies = StringHelper.GetPropertyValueByLabel("Кінокомпанія:", labels, infos);
                        Movies[i].Genre = StringHelper.GetPropertyValueByLabel("Жанр:", labels, infos);
                        Movies[i].Countries = StringHelper.GetPropertyValueByLabel("Країна:", labels, infos);
                        Movies[i].Length = StringHelper.GetPropertyValueByLabel("Тривалість:", labels, infos, "00:00:00");

                        Movies[i].ImdbRate = Convert.ToDouble(imdb);
                        Movies[i].Story = HtmlHelper.StripHtml(story.Substring(0, story.IndexOf("<div class=\"m-info\">"))).Replace("\t", "").Replace("\n", "").Replace("  ", " ");

                        var currentDesc = labels.FirstOrDefault(l => l.InnerText == "Рік:");
                        int.TryParse(infos[labels.IndexOf(currentDesc)]?.InnerText ?? "0", out var year);
                        Movies[i].Year = year;
                        Movies[i].Poster = poster;
                    });

                    //Getting comments
                    int page = 1;
                    bool finished = false;
                    do
                    {
                        using (var httpClient = new HttpClient())
                        {
                            var url = $"https://uafilm.tv/engine/ajax/comments.php?cstart={page}&news_id={Movies[i].Id}&skin=uafilm&massact=disable";
                            var json = await httpClient.GetStringAsync(url);
                            var response = JsonConvert.DeserializeObject<CommentResponse>(json);
                            if (!string.IsNullOrEmpty(response.comments))
                            {
                                await Task.Run(() =>
                                {
                                    doc.LoadHtml(response.comments);
                                });

                                var commentAuthors = doc.DocumentNode.SelectNodes("//div[@class='comm-author']");
                                var commentTexts = doc.DocumentNode.SelectNodes("//div[@class='comm-body clearfix']/div");
                                var commentDates = doc.DocumentNode.SelectNodes("//div[@class='comm-num']");
                                for (int j = 0; j < commentAuthors.Count; ++j)
                                {
                                    var date = commentDates[j].InnerText.GetDateTime();
                                    var comment = new Comment(Convert.ToInt32(Regex.Match(commentTexts[j].Id, "[0-9]+")?.Value ?? "0"), commentAuthors[j].InnerText, commentTexts[j].InnerText, date);
                                    Comments.Add(comment);
                                    Movies[i].Comments.Add(comment);
                                }
                                ++page;
                            }
                            else
                            {
                                finished = true;
                            }
                        }
                    } while (!finished);

                    ++Progress;
                    Thread.Sleep(SleepTime);
                }
                catch (Exception exc)
                {
                    ++ErrorsCount;
                    MessageBox.Show(exc.Message + "\n" + Movies[i].Name + "\n" + Movies[i].Link, "Виникла помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowMovie(object parameter)
        {
            if (parameter != null)
            {
                MovieWindow tw = new MovieWindow(parameter as Movie);
                tw.Show();
                tw.Owner = ((MainWindow)System.Windows.Application.Current.MainWindow);
            }
        }

        private void OpenFromBinary(object parameter)
        {
            Movies.Clear();
            Comments.Clear();
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "bin(*.bin)|*.bin";
            if (ofd.ShowDialog() ?? true)
            {
                using (Stream reader = File.Open(ofd.FileName, FileMode.Open))
                {
                    BinaryFormatter ser = new BinaryFormatter();
                    Movies = (ObservableCollection<Movie>)ser.Deserialize(reader);
                    foreach (Movie mv in Movies)
                    {
                        foreach (Comment cm in mv.Comments)
                        {
                            Comments.Add(cm);
                        }
                    }
                    OnPropertyChanged(nameof(Movies));
                    OnPropertyChanged(nameof(Comments));
                }
            }
        }

        private void SaveToBinary(object parameter)
        {
            Microsoft.Win32.SaveFileDialog svd = new Microsoft.Win32.SaveFileDialog();
            svd.Filter = "bin(*.bin)|*.bin";
            if (svd.ShowDialog() ?? true)
            {
                using (FileStream fileStr = new FileStream(svd.FileName, FileMode.Create))
                {
                    BinaryFormatter binFormater = new BinaryFormatter();
                    binFormater.Serialize(fileStr, Movies);
                }
            }
        }

        private void OpenSaveTrainCommentsWindow(object parameter)
        {
            if (Comments.Count != 0)
            {
                IsSavingMode = true;
                Title = "Збереження коментарів";
                StartCommand = new Command(SaveTrainComments);
                PagesWindow ppw = new PagesWindow(this);
                ppw.Show();
                ppw.Owner = (MainWindow)System.Windows.Application.Current.MainWindow;
            }
        }

        private void OpenSaveTestCommentsWindow(object parameter)
        {
            if (Comments.Count != 0)
            {
                IsSavingMode = true;
                Title = "Збереження коментарів";
                StartCommand = new Command(SaveTestComments);
                PagesWindow ppw = new PagesWindow(this);
                ppw.Show();
                ppw.Owner = (MainWindow)System.Windows.Application.Current.MainWindow;
            }
        }

        private void SaveTrainComments(object parameter)
        {
            if (Comments.Count > 0)
            {
                Directory.CreateDirectory("parse");
                var fileName = $"parse\\train_comments_{DateTime.Now.ToString().Replace(":", "").Replace(" ", "_")}.bin";
                int toId = Comments.Count - From;
                int fromId = Comments.Count - To;
                using (FileStream fileStr = new FileStream(Path.Combine(BaseDirectory, fileName), FileMode.Create))
                {
                    BinaryFormatter binFormater = new BinaryFormatter();
                    binFormater.Serialize(fileStr, new ObservableCollection<Comment>(Comments.Skip(fromId).Take(toId - fromId + 1)));
                }
                CommonInfo.TrainFile = Path.Combine(BaseDirectory, fileName);
                OnPropertyChanged(nameof(TrainCommentsGeneratedTitle));
                OnPropertyChanged(nameof(TrainCommentsGeneratedColor));
            }
        }

        private void SaveTestComments(object parameter)
        {
            if (Comments.Count > 0)
            {
                Directory.CreateDirectory("parse");
                var fileName = $"parse\\test_comments_{DateTime.Now.ToString().Replace(":", "").Replace(" ", "_")}.bin";
                int toId = Comments.Count - From;
                int fromId = Comments.Count - To;
                using (FileStream fileStr = new FileStream(Path.Combine(BaseDirectory, fileName), FileMode.Create))
                {
                    BinaryFormatter binFormater = new BinaryFormatter();
                    binFormater.Serialize(fileStr, new ObservableCollection<Comment>(Comments.Skip(fromId).Take(toId - fromId + 1)));
                }
                CommonInfo.TestFile = Path.Combine(BaseDirectory, fileName);
                OnPropertyChanged(nameof(TestCommentsGeneratedTitle));
                OnPropertyChanged(nameof(TestCommentsGeneratedColor));
            }
        }

        private void DeleteComment(object parameter)
        {
            try
            {
                Comments.Remove(parameter as Comment);
                var movie = Movies.FirstOrDefault(m => m.Comments.Contains(parameter as Comment));
                if (movie == null)
                {
                    throw new Exception("Неможливо видалити коментар, не знайдено фільм до нього");
                }
                movie.Comments.Remove(parameter as Comment);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка видалення", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveNonUkrainian(object parameter)
        {
            var count = 0;
            for (int i = 0; i < Movies.Count; ++i)
            {
                for (int j = 0; j < Movies[i].Comments.Count; ++j)
                {
                    var comment = Movies[i].Comments[j];
                    if (comment.CommentText.Contains('ы') || comment.CommentText.Contains('Ы') ||
                       comment.CommentText.Contains('ё') || comment.CommentText.Contains('Ё') ||
                       comment.CommentText.Contains('ъ') || comment.CommentText.Contains('Ъ') ||
                       comment.CommentText.Contains('э') || comment.CommentText.Contains('Э') ||
                       comment.CommentText.Contains(" c ") || comment.CommentText.Contains(" и ") ||
                       comment.CommentText.Contains("фильм") || comment.CommentText.Contains("кино") ||
                       comment.CommentText.Contains("Фильм") || comment.CommentText.Contains("Кино"))
                    {
                        Movies[i].Comments.Remove(comment);
                        Comments.Remove(comment);
                        --j;
                        ++count;
                    }
                }
            }
            MessageBox.Show($"Видалено: {count} коментарів", "Видалення");
        }

        private async void LoadResult(object parameter)
        {
            try
            {
                using (var stream = new StreamReader(new FileStream("test/result.tsv", FileMode.Open)))
                {
                    await stream.ReadLineAsync();
                    while (!stream.EndOfStream)
                    {
                        var line = await stream.ReadLineAsync();
                        var lineValues = line.Split('\t');
                        int.TryParse(lineValues[0].Replace("\"", string.Empty), out var id);
                        int.TryParse(lineValues[1].Replace("\"", string.Empty), out var sentiment);
                        var comment = Comments.FirstOrDefault(x => x.Id == id);
                        if (comment != null)
                        {
                            comment.Sentiment = sentiment;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCheckSpellingWindow(object parameter)
        {
            if (Comments.Count != 0)
            {
                Title = "Виправлення помилок";
                StartCommand = new Command(CheckSpelling);
                PagesWindow ppw = new PagesWindow(this);
                ppw.Show();
                ppw.Owner = (MainWindow)Application.Current.MainWindow;
            }
        }

        private async void CheckSpelling(object paramter)
        {
            try
            {
                ErrorsCount = 0;
                Progress = 0;
                Maximum = To - From + 1;
                int fromIndex = Comments.Count - From;
                int toIndex = Comments.Count - To;
                int correctedCommentsCount = 0;
                string correctedText = string.Empty;
                for (int i = fromIndex; i >= toIndex; --i)
                {
                    await Task.Run(() =>
                    {
                        correctedText = FixSpelling(Comments[i].CommentText);
                        Thread.Sleep(SleepTime);
                    });
                    if (Comments[i].CommentText != correctedText)
                    {
                        Comments[i].CommentText = correctedText;
                        ++correctedCommentsCount;
                    }
                    Progress++;
                }
                MessageBox.Show($"Виправлено {correctedCommentsCount} коментарів", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Виникла помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FixSpelling(string text)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.languagetool.org/v2/check");

            string postData = "disabledRules=WHITESPACE_RULE";
            postData += "&allowIncompleteResults=true";
            postData += "&enableHiddenRules=true";
            postData += "&useragent=ltorg";
            postData += $"&text={HttpUtility.UrlEncode(text)}";
            postData += "&language=uk";
            postData += "&altLanguages=en-US";
            byte[] data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            NlpResponse result = JsonConvert.DeserializeObject<NlpResponse>(json);
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            foreach (var match in result.matches)
            {
                if (match.shortMessage == "Орфографічна помилка")
                {
                    replacements[text.Substring(match.offset, match.length)] = match.replacements.FirstOrDefault()?.value ?? string.Empty;
                }
            }

            foreach (var replace in replacements)
            {
                text = text.Replace(replace.Key, replace.Value);
            }
            return text;
        }
    }
}
