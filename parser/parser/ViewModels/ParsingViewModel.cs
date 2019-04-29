using HtmlAgilityPack;
using parser.Helpers;
using parser.Models;
using parser.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

        public ParsingViewModel()
        {
            IsMoviesMode = true;
            Url = @"https://uafilm.tv/films/";
            FromPage = 1;
            ToPage = 1;
            Movies = new ObservableCollection<Movie>();
            GetAllInfoCommand = new Command(GetAllInfo);
            OpenParsingMovieWindowCommand = new Command(OpenParsingMovieWindow);
            StartParsingCommand = new Command(StartParsing);
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
                        Movie toAdd = new Movie(name, urls[i].GetAttributeValue("href", null));
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
                    var ukrName = doc.DocumentNode.SelectSingleNode("//div[@id='mc-right']/h1").InnerText;
                    var originalName = doc.DocumentNode.SelectSingleNode("//div[@id='mc-right']/span").InnerText;
                    Movies[i].Name = $"{ukrName} - {originalName}"; //?????

                    var info = doc.DocumentNode.SelectNodes("//div[@class='mi-desc']");
                    Movies[i].Director = info[0].InnerText;
                    Movies[i].Actors = info[1].InnerText;
                    Movies[i].Companies = info[2].InnerText;
                    Movies[i].Genre = info[3].InnerText;
                    Movies[i].Countries = info[4].InnerText;
                    Movies[i].Year = Convert.ToInt32(info[5].InnerText);
                    Movies[i].Length = info[6].InnerText;

                    Movies[i].ImdbLink = "DEL";
                    Movies[i].Poster = doc.DocumentNode.SelectSingleNode("//div[@class='m-img']/img").Attributes["src"].Value;
                    var story = doc.DocumentNode.SelectSingleNode("//div[@class='m-desc full-text clearfix']").InnerHtml;
                    Movies[i].Story = HtmlHelper.StripHtml(story.Substring(0, story.IndexOf("<div class=\"m-info\">"))).Replace("\t", "").Replace("\n","").Replace("  ", " ");
                   
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
    }
}
