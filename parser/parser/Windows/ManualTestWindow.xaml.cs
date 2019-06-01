using parser.ViewModels;
using System.Windows;

namespace parser.Windows
{
    public partial class ManualTestWindow : Window
    {
        public ManualTestWindow()
        {
            DataContext = new ManualTestViewModel();
            InitializeComponent();
        }
    }
}
