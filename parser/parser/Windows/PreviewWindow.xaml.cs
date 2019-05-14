using parser.ViewModels;
using System.Windows;

namespace parser.Windows
{
    public partial class PreviewWindow : Window
    {
        public PreviewWindow(TrainingViewModel model)
        {
            InitializeComponent();
            DataContext = model;
        }
    }
}
