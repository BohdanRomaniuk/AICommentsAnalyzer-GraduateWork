using parser.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public TestViewModel(CommonInfoModel _commonInfo)
        {
            CommonInfo = _commonInfo;
        }
    }
}
