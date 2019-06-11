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

        public SettingsViewModel()
        {
            Height = Settings.Default.WindowHeight;
            Width = Settings.Default.WindowWidth;
            WindowOpacity = Settings.Default.WindowOpacity;
            WindowTopmost = Settings.Default.WindowTopmost;
            SoftwareRenderingEnabled = Settings.Default.SoftwareRenderingEnabled;
            EnableDetailedDamageInfo = Settings.Default.EnableDetailedDamageInfo;
            ChartVisibility = Settings.Default.ChartVisibility;
            DetailedGridLeftWidth = new GridLength(Settings.Default.DetailedParseLeftColumnWidth, GridUnitType.Star);
            DetailedGridRightWidth = new GridLength(Settings.Default.DetailedParseRightColumnWidth, GridUnitType.Star);
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
        }

        void onSoftwareRenderingChanged()
        {
            RenderOptions.ProcessRenderMode = SoftwareRenderingEnabled ? System.Windows.Interop.RenderMode.SoftwareOnly : System.Windows.Interop.RenderMode.Default;
        }
    }
}
