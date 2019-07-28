using ApexParse.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ApexParse.ViewModel
{
    class SettingsViewModel : ViewModelBase
    {
        private int _height;
        public int Height
        {
            get { return _height; }
            set { CallerSetProperty(ref _height, value); }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set { CallerSetProperty(ref _width, value); }
        }

        float _windowOpacity;
        public float WindowOpacity
        {
            get { return _windowOpacity; }
            set { CallerSetProperty(ref _windowOpacity, value); }
        }

        bool _topmostWindow;
        public bool WindowTopmost
        {
            get { return _topmostWindow; }
            set { CallerSetProperty(ref _topmostWindow, value); }
        }

        bool _softwareRenderingEnabled;
        public bool SoftwareRenderingEnabled
        {
            get { return _softwareRenderingEnabled; }
            set { CallerSetProperty(ref _softwareRenderingEnabled, value); onSoftwareRenderingChanged(); }
        }

        GridLength _detailedGridLeftWidth;
        public GridLength DetailedGridLeftWidth
        {
            get { return _detailedGridLeftWidth; }
            set { CallerSetProperty(ref _detailedGridLeftWidth, value); }
        }

        GridLength _detailedGridRightWidth;
        public GridLength DetailedGridRightWidth
        {
            get { return _detailedGridRightWidth; }
            set { CallerSetProperty(ref _detailedGridRightWidth, value); }
        }

        bool _enableDetailedDamageInfo;
        public bool EnableDetailedDamageInfo
        {
            get { return _enableDetailedDamageInfo; }
            set { CallerSetProperty(ref _enableDetailedDamageInfo, value); }
        }

        Visibility _chartVisibility;
        public Visibility ChartVisibility
        {
            get { return _chartVisibility; }
            set { CallerSetProperty(ref _chartVisibility, value); }
        }

        bool _hideBorder;
        public bool HideBorder
        {
            get { return _hideBorder; }
            set { CallerSetProperty(ref _hideBorder, value); onHideBorderChanged(); }
        }

        bool _renderWindow;
        public bool RenderWindow
        {
            get { return _renderWindow; }
            set { CallerSetProperty(ref _renderWindow, value); }
        }

        bool _autoEndSession;
        public bool AutoEndSession
        {
            get { return _autoEndSession; }
            set { CallerSetProperty(ref _autoEndSession, value); onAutoEndChanged(); }
        }

        public event EventHandler OnAutoEndSessionChanged;

        bool initialized = false;

        public SettingsViewModel()
        {
            Height = Settings.Default.WindowHeight;
            Width = Settings.Default.WindowWidth;
            WindowOpacity = Settings.Default.WindowOpacity;
            WindowTopmost = Settings.Default.WindowTopmost;
            SoftwareRenderingEnabled = Settings.Default.SoftwareRenderingEnabled;
            EnableDetailedDamageInfo = Settings.Default.EnableDetailedDamageInfo;
            ChartVisibility = Settings.Default.ChartVisibility;
            HideBorder = Settings.Default.HideBorder;
            RenderWindow = Settings.Default.RenderWindow;
            DetailedGridLeftWidth = new GridLength(Settings.Default.DetailedParseLeftColumnWidth, GridUnitType.Star);
            DetailedGridRightWidth = new GridLength(Settings.Default.DetailedParseRightColumnWidth, GridUnitType.Star);
            AutoEndSession = Settings.Default.AutoEndSession;
            initialized = true;
        }

        public void SaveSettings()
        {
            Settings.Default.WindowWidth = Width;
            Settings.Default.WindowHeight = Height;
            Settings.Default.WindowOpacity = WindowOpacity;
            Settings.Default.WindowTopmost = WindowTopmost;
            Settings.Default.SoftwareRenderingEnabled = SoftwareRenderingEnabled;
            Settings.Default.EnableDetailedDamageInfo = EnableDetailedDamageInfo;
            Settings.Default.DetailedParseRightColumnWidth = DetailedGridRightWidth.Value;
            Settings.Default.DetailedParseLeftColumnWidth = DetailedGridLeftWidth.Value;
            Settings.Default.ChartVisibility = ChartVisibility;
            Settings.Default.HideBorder = HideBorder;
            Settings.Default.RenderWindow = RenderWindow;
            Settings.Default.AutoEndSession = AutoEndSession;
        }

        void onSoftwareRenderingChanged()
        {
            RenderOptions.ProcessRenderMode = SoftwareRenderingEnabled ? System.Windows.Interop.RenderMode.SoftwareOnly : System.Windows.Interop.RenderMode.Default;
        }

        void onAutoEndChanged()
        {
            OnAutoEndSessionChanged?.Invoke(this, EventArgs.Empty);
        }

        void onHideBorderChanged()
        {
            if (!initialized) return;
            MessageBox.Show("Hide Border setting changed. ApexParse must be restarted for this change to take effect.", "Restart Needed", MessageBoxButton.OK); //not sure why this doesn't update correctly. Look into later.
        }
    }
}
