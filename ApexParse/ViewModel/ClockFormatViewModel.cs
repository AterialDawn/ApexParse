using ApexParse.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse.ViewModel
{
    class ClockFormatViewModel : ViewModelBase
    {
        public RelayCommand<object> SetFullDateTimeFormat { get; private set; }
        private bool _fullSet = false;
        public bool FullDateTimeSet
        {
            get { return _fullSet; }
            set { CallerSetProperty(ref _fullSet, value); }
        }
        
        public RelayCommand<object> SetLongTimeFormat { get; private set; }
        private bool _longSet = false;
        public bool LongTimeSet
        {
            get { return _longSet; }
            set { CallerSetProperty(ref _longSet, value); }
        }
        public RelayCommand<object> SetShortTimeFormat { get; private set; }
        private bool _shortSet = false;
        public bool ShortTimeSet
        {
            get { return _shortSet; }
            set { CallerSetProperty(ref _shortSet, value); }
        }

        public string FormatString { get; private set; }

        public ClockFormatViewModel()
        {
            SetFullDateTimeFormat = new RelayCommand<object>((_) => setTimeFormat("F"));
            SetLongTimeFormat = new RelayCommand<object>((_) => setTimeFormat("T"));
            SetShortTimeFormat = new RelayCommand<object>((_) => setTimeFormat("t"));
            FormatString = Settings.Default.ClockFormatString;
            validate();
        }

        public void Save()
        {
            Settings.Default.ClockFormatString = FormatString;
        }

        void validate()
        {
            _longSet = false;
            _shortSet = false;
            _fullSet = false;
            switch (FormatString)
            {
                case "T":
                    _longSet = true;
                    break;
                case "t":
                    _shortSet = true;
                    break;
                case "F":
                    _fullSet = true;
                    break;
                default:
                    {
                        FormatString = "T";
                        _fullSet = true;
                        break;
                    }
            }
        }

        void setTimeFormat(string formatString)
        {
            FormatString = formatString;
            validate();
        }
    }
}
