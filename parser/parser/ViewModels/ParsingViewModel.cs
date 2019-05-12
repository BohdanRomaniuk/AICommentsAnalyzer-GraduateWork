using HtmlAgilityPack;
using Newtonsoft.Json;
using parser.Helpers;
using parser.Models;
using parser.Models.Json;
using parser.Windows;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace parser.ViewModels
{
    public class ParsingViewModel : BaseViewModel
    {
        private bool isMoviesMode;
        private string url;
        private int fromPage;
        private int toPage;
        private bool isSavingMode;
        private string savingFormat;

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

        //ProgressBar
        #region progress bar
        private int progress;
        private int maximum;
        private int errorsCount;
        public int Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        public int Maximum
        {
            get => maximum;
            set
            {
                maximum = value;
                OnPropertyChanged(nameof(Maximum));
            }
        }
        public int ErrorsCount
        {
            get => errorsCount;
            set
            {
                errorsCount = value;
                OnPropertyChanged(nameof(ErrorsCount));
            }
        }
        #endregion progress bar

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
                OnPropertyChanged(nameof(IsSavingFormatVisible));
                OnPropertyChanged(nameof(IsSleepTimeVisible));
            }
        }
        public Visibility IsSavingFormatVisible => IsSavingMode ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsSleepTimeVisible => !IsSavingMode ? Visibility.Visible : Visibility.Collapsed;
        public string SavingFormat
        {
            get => savingFormat;
            set
            {
                savingFormat = value;
                OnPropertyChanged(nameof(SavingFormat));
            }
        }
        #endregion

        public ObservableCollection<Movie> Movies { get; set; }
        public ObservableCollection<Comment> Comments { get; set; }

        public ICommand GetAllInfoCommand { get; }
        public ICommand OpenParsingMovieWindowCommand { get; }
        public ICommand StartCommand { get; private set; }
        public ICommand ShowMovieCommand { get; }

        public ICommand OpenFromBinaryCommand { get; }
        public ICommand SaveToBinaryCommand { get; }

        public ICommand MarkAsPositiveCommand { get; }
        public ICommand MarkAsNegativeCommand { get; }

        public ICommand OpenSaveCommentsWindowCommand { get; }

        public ICommand RunCMD { get; }

        public ParsingViewModel()
        {
            IsMoviesMode = true;
            Url = @"https://uafilm.tv/films/";
            FromPage = 1;
            ToPage = 1;
            IsSavingMode = false;
            SavingFormat = "*.csv";
            Movies = new ObservableCollection<Movie>();
            Comments = new ObservableCollection<Comment>();
            GetAllInfoCommand = new Command(GetAllInfo);
            OpenParsingMovieWindowCommand = new Command(OpenParsingMovieWindow);
            ShowMovieCommand = new Command(ShowMovie);
            OpenFromBinaryCommand = new Command(OpenFromBinary);
            SaveToBinaryCommand = new Command(SaveToBinary);
            MarkAsPositiveCommand = new Command(MarkAsPositive);
            MarkAsNegativeCommand = new Command(MarkAsNegative);
            OpenSaveCommentsWindowCommand = new Command(OpenSaveCommentsWindow);

            RunCMD = new Command(Run);
        }

        private void Run(object parameter)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "C:/Users/Bohdan/AppData/Local/Programs/Python/Python36/python.exe";
            var cmd = "C:/test.py";
            var args = "";
            start.Arguments = string.Format("{0}", cmd, args);
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

        private void MarkAsPositive(object parameter)
        {
            (parameter as Comment).Sentiment = 1;
        }

        private void MarkAsNegative(object parameter)
        {
            (parameter as Comment).Sentiment = 0;
        }

        private async void GetAllInfo(object parameter)
        {
            Movies.Clear();
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

                        Movies[i].Director = StringHelper.GetPropertyValueByLabel("Режисер:", labels, infos);
                        Movies[i].Actors = StringHelper.GetPropertyValueByLabel("В ролях:", labels, infos);
                        Movies[i].Companies = StringHelper.GetPropertyValueByLabel("Кінокомпанія:", labels, infos);
                        Movies[i].Genre = StringHelper.GetPropertyValueByLabel("Жанр:", labels, infos);
                        Movies[i].Countries = StringHelper.GetPropertyValueByLabel("Країна:", labels, infos);
                        var currentDesc = labels.FirstOrDefault(l => l.InnerText == "Рік:");
                        Movies[i].Year = currentDesc != null ? Convert.ToInt32(infos[labels.IndexOf(currentDesc)]?.InnerText ?? "0") : 0;
                        Movies[i].Length = StringHelper.GetPropertyValueByLabel("Тривалість:", labels, infos, "00:00:00");

                        Movies[i].ImdbRate = Convert.ToDouble(doc.DocumentNode.SelectSingleNode("//div[@class='mr-item-rate']")?.InnerText?.Trim()?.Replace('.', ',') ?? "0");
                        Movies[i].Poster = doc.DocumentNode.SelectSingleNode("//div[@class='m-img']/img")?.Attributes["src"]?.Value ?? "no-poster.jpg";
                        var story = doc.DocumentNode.SelectSingleNode("//div[@class='m-desc full-text clearfix']")?.InnerHtml ?? "<div class=\"m-info\">";
                        Movies[i].Story = HtmlHelper.StripHtml(story.Substring(0, story.IndexOf("<div class=\"m-info\">"))).Replace("\t", "").Replace("\n", "").Replace("  ", " ");
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
                                    var comment = new Comment(Convert.ToInt32(Regex.Match(commentTexts[j].Id,"[0-9]+")?.Value ?? "0"), commentAuthors[j].InnerText, commentTexts[j].InnerText, date);
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
                    System.Windows.MessageBox.Show(exc.Message + "\n" + Movies[i].Name + "\n" + Movies[i].Link, "Виникла помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void OpenSaveCommentsWindow(object parameter)
        {
            if (Comments.Count != 0)
            {
                IsSavingMode = true;
                Title = "Збереження коментарів";
                StartCommand = new Command(SaveComments);
                PagesWindow ppw = new PagesWindow(this);
                ppw.Show();
                ppw.Owner = ((MainWindow)System.Windows.Application.Current.MainWindow); ;
            }
        }

        private async void SaveComments(object parameter)
        {
            Microsoft.Win32.SaveFileDialog svd = new Microsoft.Win32.SaveFileDialog();
            svd.Filter = $"{SavingFormat.Substring(2, SavingFormat.Length - 2)}({SavingFormat})|{SavingFormat}";
            var separator = SavingFormat == "*.csv" ? ',' : '\t';
            if (svd.ShowDialog() ?? true)
            {
                using (StreamWriter fileStr = new StreamWriter(new FileStream(svd.FileName, FileMode.Create)))
                {
                    ErrorsCount = 0;
                    Progress = 0;
                    Maximum = To - From + 1;
                    int fromIndex = Comments.Count - From;
                    int toIndex = Comments.Count - To;
                    for (int i = fromIndex; i >= toIndex; --i)
                    {
                        await fileStr.WriteLineAsync($"{Comments[i].CommentText}{separator}{Comments[i].Sentiment}");
                        ++Progress;
                    }
                }
            }
        }
    }
}
