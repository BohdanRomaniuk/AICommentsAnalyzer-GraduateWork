using Microsoft.Win32;
using parser.Helpers;
using parser.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace parser.ViewModels
{
    public class MovieWindowViewModel : BaseViewModel
    {
        private Movie currentMovie;
        private BitmapImage posterImage;
        public Movie CurrentMovie
        {
            get
            {
                return currentMovie;
            }
            set
            {
                currentMovie = value;
                OnPropertyChanged(nameof(CurrentMovie));
            }
        }
        public BitmapImage PosterImage
        {
            get
            {
                return posterImage;
            }
            set
            {
                posterImage = value;
                OnPropertyChanged(nameof(PosterImage));
            }
        }

        public ICommand CopyPosterUrlCommand { get; private set; }
        public ICommand SavePosterAsCommand { get; private set; }

        public ICommand MarkAsPositiveCommand { get; }
        public ICommand MarkAsNegativeCommand { get; }

        public MovieWindowViewModel(Movie _currentMovie, bool _offlineMode = false, string _postersLocation = "")
        {
            try
            {
                CurrentMovie = _currentMovie;
                if (_currentMovie.Poster != "немає")
                {
                    if (_offlineMode)
                    {
                        PosterImage = new BitmapImage(new Uri(_postersLocation + "\\" + _currentMovie.PosterFileName));
                    }
                    else
                    {
                        PosterImage = new BitmapImage(new Uri(_currentMovie.Poster));
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message + "\n" + CurrentMovie.Name + "\n" + CurrentMovie.Link, "Виникла помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            CopyPosterUrlCommand = new Command(CopyPosterUrl);
            SavePosterAsCommand = new Command(SavePosterAs);

            MarkAsPositiveCommand = new Command(MarkAsPositive);
            MarkAsNegativeCommand = new Command(MarkAsNegative);
        }

        public void CopyPosterUrl(object obj)
        {
            Clipboard.Clear();
            Clipboard.SetText(CurrentMovie.Poster);
        }

        public async void SavePosterAs(object obj)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = CurrentMovie.PosterFileName;
            saveFileDialog.Filter = "Картинки (*.jpg, *.png, *.gif, *.jpeg, *.bmp)|*.jpg;*.png;*.gif;*.jpeg;*.bmp|Усі файли (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileName = saveFileDialog.FileName;
                    await Task.Run(() =>
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile(CurrentMovie.Poster, fileName);
                    });
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message + "\n" + CurrentMovie.Name + "\n" + CurrentMovie.Link, "Виникла помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
