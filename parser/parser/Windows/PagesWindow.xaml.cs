using parser.ViewModels;
using System.Windows;

namespace parser.Windows
{
    public partial class PagesWindow : Window
    {
        public PagesWindow()
        {
            InitializeComponent();
        }

        public PagesWindow(ParsingViewModel context)
        {
            InitializeComponent();
            DataContext = context;
        }
    }
}
