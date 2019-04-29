using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace parser.Models
{
    [Serializable]
    public sealed class Movie : INotifyPropertyChanged, IEquatable<Movie>
    {
        private int id; //uafilm id for comment parsing
        private string ukrName;
        private string originalName;
        private string link;
        private int year;
        private string genre;
        private string countries;
        private string length;
        private double imdbRate;
        private string companies;
        private string director;
        private string actors;
        private string story;
        private string poster;
        private string posterFileName;

        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        public string UkrName
        {
            get => ukrName;
            set
            {
                ukrName = value;
                OnPropertyChanged(nameof(UkrName));

                PosterFileName = CreatePosterFileName(Name + " (" + Year + ")", Poster);
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(PosterFileName));
            }
        }
        public string OriginalName
        {
            get => originalName;
            set
            {
                originalName = value;
                OnPropertyChanged(nameof(OriginalName));

                PosterFileName = CreatePosterFileName(Name + " (" + Year + ")", Poster);
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(PosterFileName));
            }
        }
        public string Name => $"{ukrName} - {originalName}";
        public string Link
        {
            get => link;
            set
            {
                link = value;
                OnPropertyChanged(nameof(Link));
            }
        }
        public int Year
        {
            get => year;
            set
            {
                year = value;
                OnPropertyChanged(nameof(Year));
            }
        }
        public string Genre
        {
            get => genre;
            set
            {
                genre = value;
                OnPropertyChanged(nameof(Genre));
            }
        }
        public string Countries
        {
            get => countries;
            set
            {
                countries = value;
                OnPropertyChanged(nameof(Countries));
            }
        }
        public string Length
        {
            get => length;
            set
            {
                length = value;
                OnPropertyChanged(nameof(Length));
            }
        }
        public double ImdbRate
        {
            get => imdbRate;
            set
            {
                imdbRate = value;
                OnPropertyChanged(nameof(ImdbRate));
            }
        }
        public string Companies
        {
            get => companies;
            set
            {
                companies = value;
                OnPropertyChanged(nameof(Companies));
            }
        }
        public string Director
        {
            get => director;
            set
            {
                director = value;
                OnPropertyChanged(nameof(Director));
            }
        }
        public string Actors
        {
            get => actors;
            set
            {
                actors = value;
                OnPropertyChanged(nameof(Actors));
            }
        }
        public string Story
        {
            get => story;
            set
            {
                story = value;
                OnPropertyChanged(nameof(Story));
            }
        }
        public string Poster
        {
            get => poster;
            set
            {
                poster = value;
                PosterFileName = CreatePosterFileName(Name + " (" + Year + ")", value);
                OnPropertyChanged(nameof(Poster));
                OnPropertyChanged(nameof(PosterFileName));
            }
        }
        public string PosterFileName
        {
            get => posterFileName;
            set
            {
                posterFileName = value;
                OnPropertyChanged(nameof(PosterFileName));
            }
        }
        public ObservableCollection<Genre> MovieGenres { get; set; }
        public ObservableCollection<Country> MovieCountries { get; set; }
        public ObservableCollection<Comment> Comments { get; set; }

        public Movie()
        {
            MovieGenres = new ObservableCollection<Genre>();
            MovieCountries = new ObservableCollection<Country>();
            Comments = new ObservableCollection<Comment>();
        }

        public Movie(string _ukrName, string _link = "http://", int _year = 0, string _genre = "немає", string _countries = "відсутні", string _length = "00:00:00", double _imdb = 0, string _companies = "відсутні", string _director = "немає", string _actors = "немає", string _story = "немає", string _poster = "немає", string _posterFileName = "немає")
        {
            UkrName = _ukrName;
            Link = _link;
            Year = _year;
            Genre = _genre;
            Countries = _countries;
            Length = _length;
            ImdbRate = _imdb;
            Companies = _companies;
            Director = _director;
            Actors = _actors;
            Story = _story;
            Poster = _poster;
            PosterFileName = _posterFileName;

            MovieGenres = new ObservableCollection<Genre>();
            MovieCountries = new ObservableCollection<Country>();
            Comments = new ObservableCollection<Comment>();
        }

        public override string ToString()
        {
            return string.Format("{0,-10}{1,-10}{2,-10}{3,-10}{4,-10}{5,-10}{6,-10}{7,-10}{8,-10}{9,-10}{10,-10}", Name, Link, Year, Genre, Countries, Length, ImdbRate, Companies, Director, Actors, Story, Poster, PosterFileName);
        }

        private static string CreatePosterFileName(string _name, string _url)
        {
            _name = _name.Replace('/', '-');
            _name = _name.Replace('|', '-');
            _name = _name.Replace(':', ' ');
            _name = _name.Replace('*', ' ');
            _name = _name.Replace('?', ' ');
            _name = _name.Replace('"', ' ');
            _name = _name.Replace('<', ' ');
            _name = _name.Replace('>', ' ');
            _name = _name.Replace("  ", " ");
            _name = _name.Replace(") .", ").");
            _name = _name.Replace(" - ", "-");
            _name = _name.Replace(' ', '_');
            _name = _name.Replace(Environment.NewLine, " ");
            _name = _name.Replace("\r\n", " ");
            return _name + Path.GetExtension(_url);
        }

        public bool Equals(Movie other)
        {
            return Name.Equals(other.Name);
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
