using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse.ViewModel
{
    class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        

        protected virtual bool CallerSetProperty<T>(ref T storage, T value, [CallerMemberName] string propName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            CallerPropertyChanged(propName);
            return true;
        }

        protected virtual void CallerPropertyChanged([CallerMemberName] string propName = null)
        {
            NotifyPropertyChanged(propName);
        }

        protected virtual void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
