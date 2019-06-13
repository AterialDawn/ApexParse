using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse.ViewModel
{
    class WindowOpacityVM : ViewModelBase
    {
        bool _windowTransparency20Percent;
        public bool WindowTransparency20Percent
        {
            get { return _windowTransparency20Percent; }
            set { CallerSetProperty(ref _windowTransparency20Percent, value); SetOpacity(.2f); }
        }

        bool _windowTransparency40Percent;
        public bool WindowTransparency40Percent
        {
            get { return _windowTransparency40Percent; }
            set { CallerSetProperty(ref _windowTransparency40Percent, value); SetOpacity(.4f); }
        }

        bool _windowTransparency60Percent;
        public bool WindowTransparency60Percent
        {
            get { return _windowTransparency60Percent; }
            set { CallerSetProperty(ref _windowTransparency60Percent, value); SetOpacity(.6f); }
        }

        bool _windowTransparency80Percent;
        public bool WindowTransparency80Percent
        {
            get { return _windowTransparency80Percent; }
            set { CallerSetProperty(ref _windowTransparency80Percent, value); SetOpacity(.8f); }
        }

        bool _windowTransparency100Percent;
        public bool WindowTransparency100Percent
        {
            get { return _windowTransparency100Percent; }
            set { CallerSetProperty(ref _windowTransparency100Percent, value); SetOpacity(1f); }
        }

        private SettingsViewModel settingsVM;

        public WindowOpacityVM(SettingsViewModel settings)
        {
            settingsVM = settings;
            updateOpacityProperties(settingsVM.WindowOpacity);
        }

        private void SetOpacity(float opacity)
        {
            settingsVM.WindowOpacity = opacity;
            updateOpacityProperties(opacity);
        }

        private void updateOpacityProperties(float opacity)
        {
            _windowTransparency20Percent = _windowTransparency40Percent = _windowTransparency60Percent = _windowTransparency80Percent = _windowTransparency100Percent = false;
            if (opacity == .2f) _windowTransparency20Percent = true;
            else if (opacity == .4f) _windowTransparency40Percent = true;
            else if (opacity == .6f) _windowTransparency60Percent = true;
            else if (opacity == .8f) _windowTransparency80Percent = true;
            else _windowTransparency100Percent = true;

            NotifyPropertyChanged(null);
        }
    }
}
