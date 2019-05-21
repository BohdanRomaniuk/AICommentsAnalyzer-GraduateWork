using parser.Helpers;
using parser.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace parser.ViewModels
{
    public class TestViewModel: BaseViewModel
    {
        private CommonInfoModel trainingInfo;

        public CommonInfoModel CommonInfo
        {
            get => trainingInfo;
            set
            {
                trainingInfo = value;
                OnPropertyChanged(nameof(CommonInfo));
            }
        }

        public ICommand OpenCommentsFileCommand { get; }

        public TestViewModel(CommonInfoModel _commonInfo)
        {
            CommonInfo = _commonInfo;
            OpenCommentsFileCommand = new Command(OpenCommentsFile);
        }

        private void OpenCommentsFile(object parameter)
        {
            CommonInfo.TrainComments.Clear();
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "bin(*.bin)|*.bin";
            if (ofd.ShowDialog() ?? true)
            {
                CommonInfo.TestFile = ofd.FileName;
            }
        }
    }
}
