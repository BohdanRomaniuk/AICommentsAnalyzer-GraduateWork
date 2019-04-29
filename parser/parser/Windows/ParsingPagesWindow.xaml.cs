using parser.ViewModels;
using System.Windows;

namespace parser.Windows
{
    public partial class ParsingPagesWindow : Window
    {
        public ParsingPagesWindow()
        {
            InitializeComponent();
        }

        public ParsingPagesWindow(ParsingViewModel context)
        {
            InitializeComponent();
            DataContext = context;
        }
    }
}
