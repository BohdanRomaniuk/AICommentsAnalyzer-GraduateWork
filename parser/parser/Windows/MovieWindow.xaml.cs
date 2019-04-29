using parser.Models;
using parser.ViewModels;
using System.Windows;

namespace parser.Windows
{
    public partial class MovieWindow : Window
    {
        public MovieWindow(Movie movie)
        {
            InitializeComponent();
            DataContext = new MovieWindowViewModel(movie);
        }
    }
}
