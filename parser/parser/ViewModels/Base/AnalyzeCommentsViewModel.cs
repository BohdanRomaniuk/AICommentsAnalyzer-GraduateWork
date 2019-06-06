using parser.Helpers;
using parser.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace parser.ViewModels
{
    public class AnalyzeCommentsViewModel : BaseViewModel
    {
        public ICommand MarkAsPositiveCommand { get; }
        public ICommand MarkAsNegativeCommand { get; }

        //Implementation are in inherited clases
        public ICommand DeleteCommentCommand { get; protected set; }

        public AnalyzeCommentsViewModel()
        {
            MarkAsPositiveCommand = new Command(MarkAsPositive);
            MarkAsNegativeCommand = new Command(MarkAsNegative);
        }

        private void MarkAsPositive(object parameter)
        {
            try
            {
                (parameter as Comment).Sentiment = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MarkAsNegative(object parameter)
        {
            try
            {
                (parameter as Comment).Sentiment = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
