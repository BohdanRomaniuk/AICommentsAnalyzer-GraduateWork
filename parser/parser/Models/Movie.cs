using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace parser.Models
{
    [Serializable]
    public sealed class Movie : INotifyPropertyChanged, IEquatable<Movie>
    {
        private string name;
        private string link;
        private int year;
        private string genre;
        private string countries;
        private string length;
        private string imdbLink;
        private string companies;
        private string director;
        private string actors;
        private string story;
        private string poster;
        private string posterFileName;
        private List<Genre> movieGenres;
        private List<Country> movieCountries;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                PosterFileName = CreatePosterFileName(value + " (" + Year + ")", Poster);
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(PosterFileName));
            }
        }
        public string Link
        {
            get
            {
                return link;
            }
            set
            {
                link = value;
                OnPropertyChanged(nameof(Link));
            }
        }
        public int Year
        {
            get
            {
                return year;
            }
            set
            {
                year = value;
                OnPropertyChanged(nameof(Year));
            }
        }
        public string Genre
        {
            get
            {
                return genre;
            }
            set
            {
                genre = value;
                OnPropertyChanged(nameof(Genre));
            }
        }
        public string Countries
        {
            get
            {
                return countries;
            }
            set
            {
                countries = value;
                OnPropertyChanged(nameof(Countries));
            }
        }
        public string Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
                OnPropertyChanged(nameof(Length));
            }
        }
        public string ImdbLink
        {
            get
            {
                return imdbLink;
            }
            set
            {
                imdbLink = value;
                OnPropertyChanged(nameof(ImdbLink));
            }
        }
        public string Companies
        {
            get
            {
                return companies;
            }
            set
            {
                companies = value;
                OnPropertyChanged(nameof(Companies));
            }
        }
        public string Director
        {
            get
            {
                return director;
            }
            set
            {
                director = value;
                OnPropertyChanged(nameof(Director));
            }
        }
        public string Actors
        {
            get
            {
                return actors;
            }
            set
            {
                actors = value;
                OnPropertyChanged(nameof(Actors));
            }
        }
        public string Story
        {
            get
            {
                return story;
            }
            set
            {
                story = value;
                OnPropertyChanged(nameof(Story));
            }
        }
        public string Poster
        {
            get
            {
                return poster;
            }
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
            get
            {
                return posterFileName;
            }
            set
            {
                posterFileName = value;
                OnPropertyChanged(nameof(PosterFileName));
            }
        }
        public List<Genre> MovieGenres
        {
            get
            {
                return movieGenres;
            }
            set
            {
                movieGenres = value;
                OnPropertyChanged(nameof(MovieGenres));
            }
        }
        public List<Country> MovieCountries
        {
            get
            {
                return movieCountries;
            }
            set
            {
                movieCountries = value;
                OnPropertyChanged(nameof(MovieCountries));
            }
        }

        public Movie()
        {
            MovieGenres = new List<Genre>();
            MovieCountries = new List<Country>();
        }

        public Movie(string _name, string _link = "http://", int _year = 0, string _genre = "немає", string _countries = "відсутні", string _length = "00:00:00", string _imdb = "http://", string _companies = "відсутні", string _director = "немає", string _actors = "немає", string _story = "немає", string _poster = "немає", string _posterFileName = "немає")
        {
            Name = _name;
            Link = _link;
            Year = _year;
            Genre = _genre;
            Countries = _countries;
            Length = _length;
            ImdbLink = _imdb;
            Companies = _companies;
            Director = _director;
            Actors = _actors;
            Story = _story;
            Poster = _poster;
            PosterFileName = _posterFileName;
            MovieGenres = new List<Genre>();
            MovieCountries = new List<Country>();
        }

        public override string ToString()
        {
            return String.Format("{0,-10}{1,-10}{2,-10}{3,-10}{4,-10}{5,-10}{6,-10}{7,-10}{8,-10}{9,-10}{10,-10}", Name, Link, Year, Genre, Countries, Length, ImdbLink, Companies, Director, Actors, Story, Poster, PosterFileName);
        }

        public bool Equals(Movie other)
        {
            return Name.Equals(other.Name);
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

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
