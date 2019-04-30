using HtmlAgilityPack;
using Newtonsoft.Json;
using parser.Helpers;
using parser.Models;
using parser.Models.Json;
using parser.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
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
        private int fromMovie;
        private int toMovie;
        private int sleepTime;
        public int FromMovie
        {
            get => fromMovie;
            set
            {
                fromMovie = value;
                OnPropertyChanged(nameof(FromMovie));
            }
        }
        public int ToMovie
        {
            get => toMovie;
            set
            {
                toMovie = value;
                OnPropertyChanged(nameof(ToMovie));
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
        #endregion

        public ObservableCollection<Movie> Movies { get; set; }
        public ObservableCollection<Comment> Comments { get; set; }

        public ICommand GetAllInfoCommand { get; }
        public ICommand OpenParsingMovieWindowCommand { get; }
        public ICommand StartParsingCommand { get; }
        public ICommand ShowMovieCommand { get; }

        public ICommand OpenFromBinaryCommand { get; }
        public ICommand SaveToBinaryCommand { get; }

        public ParsingViewModel()
        {
            IsMoviesMode = true;
            Url = @"https://uafilm.tv/films/";
            FromPage = 1;
            ToPage = 1;
            Movies = new ObservableCollection<Movie>();
            Comments = new ObservableCollection<Comment>();
            GetAllInfoCommand = new Command(GetAllInfo);
            OpenParsingMovieWindowCommand = new Command(OpenParsingMovieWindow);
            StartParsingCommand = new Command(StartParsing);
            ShowMovieCommand = new Command(ShowMovie);
            OpenFromBinaryCommand = new Command(OpenFromBinary);
            SaveToBinaryCommand = new Command(SaveToBinary);
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
                ParsingPagesWindow ppw = new ParsingPagesWindow(this);
                ppw.Show();
                ppw.Owner = ((MainWindow)System.Windows.Application.Current.MainWindow); ;
            }
        }

        private async void StartParsing(object parameter)
        {
            ErrorsCount = 0;
            Progress = 0;
            Maximum = ToMovie - FromMovie + 1;
            int fromIndex = Movies.Count - FromMovie;
            int toIndex = Movies.Count - ToMovie;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = new HtmlDocument();
            for (int i = fromIndex; i >= toIndex; --i)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        doc = web.Load(Movies[i].Link);
                    });
                    Movies[i].UkrName = doc.DocumentNode.SelectSingleNode("//div[@id='mc-right']/h1")?.InnerText ?? "";
                    Movies[i].OriginalName = doc.DocumentNode.SelectSingleNode("//div[@id='mc-right']/span")?.InnerText ?? "";

                    var info = doc.DocumentNode.SelectNodes("//div[@class='mi-desc']");
                    Movies[i].Director = info[0]?.InnerText ?? "";
                    Movies[i].Actors = info[1]?.InnerText ?? "";
                    Movies[i].Companies = info[2]?.InnerText ?? "";
                    Movies[i].Genre = info[3]?.InnerText ?? "";
                    Movies[i].Countries = info[4]?.InnerText ?? "";
                    Movies[i].Year = Convert.ToInt32(info[5]?.InnerText ?? "0");
                    Movies[i].Length = info[6]?.InnerText ?? "00:00:00";

                    Movies[i].ImdbRate = Convert.ToDouble(doc.DocumentNode.SelectSingleNode("//div[@class='mr-item-rate']")?.InnerText?.Trim()?.Replace('.',',') ?? "0");
                    Movies[i].Poster = doc.DocumentNode.SelectSingleNode("//div[@class='m-img']/img")?.Attributes["src"]?.Value ?? "no-poster.jpg";
                    var story = doc.DocumentNode.SelectSingleNode("//div[@class='m-desc full-text clearfix']")?.InnerHtml ?? "<div class=\"m-info\">";
                    Movies[i].Story = HtmlHelper.StripHtml(story.Substring(0, story.IndexOf("<div class=\"m-info\">"))).Replace("\t", "").Replace("\n","").Replace("  ", " ");

                    //Getting comments
                    int cstart = 1;
                    while(true)
                    {
                        using (var httpClient = new HttpClient())
                        {
                            var url = $"https://uafilm.tv/engine/ajax/comments.php?cstart={cstart}&news_id={Movies[i].Id}&skin=uafilm&massact=disable";
                            var json = await httpClient.GetStringAsync(url);
                            var response = JsonConvert.DeserializeObject<CommentResponse>(json);
                            if(string.IsNullOrEmpty(response.comments))
                            {
                                break;
                            }
                            doc.LoadHtml(response.comments);

                            var commentAuthors = doc.DocumentNode.SelectNodes("//div[@class='comm-author']");
                            var commentTexts = doc.DocumentNode.SelectNodes("//div[@class='comm-body clearfix']/div");
                            for(int j=0; j< commentAuthors.Count; ++j)
                            {
                                var comment = new Comment(commentAuthors[j].InnerText, commentTexts[j].InnerText, DateTime.Now);
                                Comments.Add(comment);
                                Movies[i].Comments.Add(comment);
                            }
                            ++cstart;
                        }
                    }

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

        private void OpenFromBinary(object obj)
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
                    foreach(Movie mv in Movies)
                    {
                        foreach(Comment cm in mv.Comments)
                        {
                            Comments.Add(cm);
                        }
                    }
                    OnPropertyChanged(nameof(Movies));
                    OnPropertyChanged(nameof(Comments));
                }
            }
        }

        private void SaveToBinary(object obj)
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
    }
}
