using parser.Helpers;
using parser.Models;
using System.Windows.Input;

namespace parser.ViewModels
{
    public class AnalyzeCommentsViewModel : BaseViewModel
    {
        public ICommand MarkAsPositiveCommand { get; }
        public ICommand MarkAsNegativeCommand { get; }

        public AnalyzeCommentsViewModel()
        {
            MarkAsPositiveCommand = new Command(MarkAsPositive);
            MarkAsNegativeCommand = new Command(MarkAsNegative);
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
