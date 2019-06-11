using ApexParse.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse.ViewModel
{
    class SessionItemVM : ViewModelBase
    {
        public string SessionName { get; private set; }

        public string SessionPath { get; private set; }

        public RelayCommand<object> OpenSessionLogCommand { get; private set; }

        public SessionItemVM(string sessionName, string sessionPath)
        {
            SessionName = sessionName;
            SessionPath = sessionPath;

            OpenSessionLogCommand = new RelayCommand<object>((_) => { openSessionLog(); });
        }

        private void openSessionLog()
        {
            UtilityMethods.OpenWithDefaultProgram(SessionPath);
        }
    }
}
