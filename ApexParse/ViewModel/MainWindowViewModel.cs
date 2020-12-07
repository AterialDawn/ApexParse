using ApexParse.Properties;
using ApexParse.Utility;
using ApexParse.Views.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ApexParse.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        public DamageParser CurrentDamageParser { get; private set; }
        public SettingsViewModel SettingsVM { get; private set; }
        public WindowOpacityVM OpacityVM { get; private set; }
        public ClockFormatViewModel ClockFormat { get; private set; }

        public RelayCommand<object> UpdateDamageLogsCommand { get; private set; }
        public RelayCommand<object> ResetTrackerCommand { get; private set; }
        public RelayCommand<object> SaveSessionCommand { get; private set; }
        public RelayCommand<object> ReselectDamageLogsCommand { get; private set; }
        public RelayCommand<object> SetAccentColorCommand { get; private set; }
        public RelayCommand<object> SetBackgroundImageCommand { get; private set; }
        public RelayCommand<string> SetWindowTransparencyCommand { get; private set; }
        public RelayCommand<object> OpenSessionLogsCommand { get; private set; }
        public RelayCommand<object> ExitCommand { get; private set; }
        public RelayCommand<object> ChangeAutoEndTimeoutCommand { get; private set; }

        public ObservableCollection<ViewModelBase> AllTabs { get; private set; } = new ObservableCollection<ViewModelBase>();

        public ObservableCollection<SessionItemVM> SavedSessions { get; private set; } = new ObservableCollection<SessionItemVM>();

        BehaviorRelayer _renderNowRelay = new BehaviorRelayer();
        public BehaviorRelayer RenderNowRelay
        {
            get { return _renderNowRelay; }
            set { CallerSetProperty(ref _renderNowRelay, value); }
        }

        bool _shortenDPSReadout;
        public bool ShortenDPSReadout
        {
            get { return _shortenDPSReadout; }
            set { CallerSetProperty(ref _shortenDPSReadout, value); onShortenDPSChanged(); }
        }

        bool _openGraphForSelfAutomatically;
        public bool OpenGraphForSelfAutomatically
        {
            get { return _openGraphForSelfAutomatically; }
            set { CallerSetProperty(ref _openGraphForSelfAutomatically, value); }
        }

        HiddenSplitTypesSettingsVM _damageTypesSettings;
        public HiddenSplitTypesSettingsVM DamageTypesSettings
        {
            get { return _damageTypesSettings; }
            set { CallerSetProperty(ref _damageTypesSettings, value); }
        }

        ColumnVisibilityVM _detailedDamageVisibleSettings;
        public ColumnVisibilityVM DetailedDamageVisibleSettings
        {
            get { return _detailedDamageVisibleSettings; }
            set { CallerSetProperty(ref _detailedDamageVisibleSettings, value); }
        }

        EnabledLineSeriesSettingsVM _lineSeriesSettings;
        public EnabledLineSeriesSettingsVM LineSeriesSettings
        {
            get { return _lineSeriesSettings; }
            set { CallerSetProperty(ref _lineSeriesSettings, value); }
        }

        private ViewModelBase _selectedTab;
        public ViewModelBase SelectedTab
        {
            get { return _selectedTab; }
            set { CallerSetProperty(ref _selectedTab, value); }
        }

        private string _statusBarText = "Waiting...";
        public string StatusBarText
        {
            get { return _statusBarText; }
            set { CallerSetProperty(ref _statusBarText, value); }
        }

        private bool _showInstantDPS;
        public bool ShowInstantDPS
        {
            get { return _showInstantDPS; }
            set { CallerSetProperty(ref _showInstantDPS, value); }
        }

        private bool _highlightDPS = true;
        public bool HighlightDPS
        {
            get { return _highlightDPS; }
            set { CallerSetProperty(ref _highlightDPS, value); onSettingsChanged(); }
        }

        private string _backgroundImagePath = null;
        public string BackgroundImagePath
        {
            get { return _backgroundImagePath; }
            set { CallerSetProperty(ref _backgroundImagePath, value); }
        }

        public bool IsParsingActive
        {
            get { return CurrentDamageParser.ParsingActive; }
        }

        //so i dont have to use a bool inverting converter in view
        public bool IsParsingInactive
        {
            get { return !IsParsingActive; }
        }

        public bool HasSessions
        {
            get { return SavedSessions.Count > 0; }
        }

        public bool HasBackgroundImage
        {
            get { return BackgroundImagePath != null; }
        }

        string _saveToImagePath;
        public string SaveToImagePath
        {
            get { return _saveToImagePath; }
            set { CallerSetProperty(ref _saveToImagePath, value); }
        }

        bool _renderNow;
        public bool RenderNow
        {
            get { return _renderNow; }
            set { CallerSetProperty(ref _renderNow, value); }
        }

        bool _anonymizePlayers;
        public bool AnonymizePlayers
        {
            get { return _anonymizePlayers; }
            set { CallerSetProperty(ref _anonymizePlayers, value); OnAnonymizePlayersChanged?.Invoke(this, EventArgs.Empty); }
        }

        bool _requestingUserInput;
        public bool RequestingUserInput
        {
            get { return _requestingUserInput; }
            set { CallerSetProperty(ref _requestingUserInput, value); }
        }

        string _currentTimeString;
        public string CurrentTimeString
        {
            get { return _currentTimeString; }
            set { CallerSetProperty(ref _currentTimeString, value); }
        }

        RequestUserInputViewModel _requestInputVM;
        public RequestUserInputViewModel RequestInputVM
        {
            get { return _requestInputVM; }
            set { CallerSetProperty(ref _requestInputVM, value); }
        }

        public bool IsZanverseSplit
        {
            get { return DamageTypesSettings.SplitZanverse; }
        }

        public event EventHandler OnAnonymizePlayersChanged;

        private GraphPlayerTabVM selfPlayerTab = null;
        private AllPlayersTabViewModel allPlayersTab = null;
        private Dictionary<PSO2Player, GraphPlayerTabVM> playerTabDict = new Dictionary<PSO2Player, GraphPlayerTabVM>();
        private object tabManipulationLock = new object();
        private HotkeyUtil hotkeyUtil;

        public MainWindowViewModel()
        {
            hotkeyUtil = new HotkeyUtil(App.Current.MainWindow); //MainWindow should be MainWindow.xaml, per ms documentation
            OnAnonymizePlayersChanged += MainWindowViewModel_OnAnonymizePlayersChanged;
            App.Current.Exit += Current_Exit;
            SettingsVM = new SettingsViewModel();
            SettingsVM.OnAutoEndSessionChanged += SettingsVM_OnAutoEndSessionChanged;
            OpacityVM = new WindowOpacityVM(SettingsVM);
            RequestInputVM = new RequestUserInputViewModel("Seconds before session auto-ends");
            RequestInputVM.UserInputAccepted += RequestInputVM_UserInputAccepted;
            RequestInputVM.ValidateUserInput += RequestInputVM_ValidateUserInput;
            RequestInputVM.OnCancelled += RequestInputVM_OnCancelled;

            UpdateDamageLogsCommand = new RelayCommand<object>((_) => selectDamageLogsPath(true));
            ResetTrackerCommand = new RelayCommand<object>((_) => resetParser());
            SaveSessionCommand = new RelayCommand<object>((_) => saveSession());
            ReselectDamageLogsCommand = new RelayCommand<object>((_) => reselectDamageLogs());
            SetAccentColorCommand = new RelayCommand<object>((_) => setAccentColor());
            SetBackgroundImageCommand = new RelayCommand<object>((_) => setBackgroundImage());
            OpenSessionLogsCommand = new RelayCommand<object>((_) => openSessionLogsFolder());
            ExitCommand = new RelayCommand<object>((_) => App.Current.Shutdown());
            ChangeAutoEndTimeoutCommand = new RelayCommand<object>((_) => changeAutoEndTimeout());

            if (string.IsNullOrWhiteSpace(Settings.Default.DamageLogsPath))
            {
                selectDamageLogsPath(false);
            }
            loadSettings();

            if (CurrentDamageParser == null)
            {
                initializeDamageParser(Settings.Default.DamageLogsPath);
            }
            try
            {
                CurrentDamageParser.Start(TimeSpan.FromSeconds(Settings.Default.ParserTickRate));
            }
            catch (Exception)
            {
                MessageBox.Show("Error starting parser! Ensure the damagelogs folder is correctly set.");
                Settings.Default.DamageLogsPath = "";
                Settings.Default.Save();
            }

            allPlayersTab = new AllPlayersTabViewModel(this);
            allPlayersTab.UserDoubleClickedEvent += AllPlayersTab_UserDoubleClickedEvent;
            AllTabs.Add(allPlayersTab);
            SelectedTab = allPlayersTab;

            initializeHotkeys();
            StatusBarText = $"Welcome to ApexParse v{App.VersionString}";

            ClockFormat = new ClockFormatViewModel();
            UpdateClockTask();
        }

        private async void UpdateClockTask()
        {
            for (; ; )
            {
                CurrentTimeString = DateTime.Now.ToString(ClockFormat.FormatString);
                DateTime n = DateTime.Now.AddSeconds(1);
                DateTime timeNextUpdateHappens = new DateTime(n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second);
                TimeSpan intervalUntilNextUpdate = timeNextUpdateHappens - DateTime.Now;

                if (intervalUntilNextUpdate.TotalMilliseconds >= 0)
                {
                    await Task.Delay(intervalUntilNextUpdate);
                }
            }
        }

        private void SettingsVM_OnAutoEndSessionChanged(object sender, EventArgs e)
        {
            CurrentDamageParser.AutoEndSession = SettingsVM.AutoEndSession;
        }

        private void CurrentDamageParser_AutoEndSessionEvent(object sender, EventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() => saveSession()));
        }

        private void RequestInputVM_OnCancelled(object sender, EventArgs e)
        {
            RequestingUserInput = false;
            Console.WriteLine("Cancelled!");
        }

        private void RequestInputVM_ValidateUserInput(object sender, ValidateInputEventArgs e)
        {
            double val;
            if (!double.TryParse(e.TextToValidate, out val))
            {
                MessageBox.Show("Input must be in seconds");
                return;
            }
            if (val < 5)
            {
                MessageBox.Show("Minimum seconds is 5, enter a number greater than 5");
                return;
            }
            Console.WriteLine("validation success!");
            e.ValidationSucceeded = true;
        }

        private void RequestInputVM_UserInputAccepted(object sender, EventArgs e)
        {
            //only time we use this for now is to handle setting timeout for now
            TimeSpan newTimeout = TimeSpan.FromSeconds(double.Parse(RequestInputVM.UserInput));
            Settings.Default.AutoEndSessionTimeout = newTimeout;
            Console.WriteLine($"Setting timeout to {newTimeout}");
            CurrentDamageParser.AutoEndTimeout = newTimeout;
            RequestingUserInput = false;
        }

        private void changeAutoEndTimeout()
        {
            if (RequestingUserInput) return;
            RequestInputVM.UserInput = Settings.Default.AutoEndSessionTimeout.TotalSeconds.ToString();
            RequestingUserInput = true;
            Console.WriteLine("Changing auto end timeout...");
        }

        private void MainWindowViewModel_OnAnonymizePlayersChanged(object sender, EventArgs e)
        {
            CurrentDamageParser?.SetNameAnonimization(AnonymizePlayers);
        }

        private void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            Console.WriteLine("Saving settings");
            Settings.Default.HighlightDPS = HighlightDPS;
            Settings.Default.BackgroundImagePath = BackgroundImagePath == null ? string.Empty : BackgroundImagePath;
            Settings.Default.EnabledLineSeries = LineSeriesSettings.GetEnumValue().ToString();
            Settings.Default.DetailedDamageVisibleColumns = DetailedDamageVisibleSettings.GetEnum().ToString();
            Settings.Default.HiddenDamageTypes = DamageTypesSettings.GetHiddenTrackersEnum().ToString();
            Settings.Default.SplitDamageTypes = DamageTypesSettings.GetSeparatedTrackersEnum(false).ToString();
            Settings.Default.ShortenDPSReadout = ShortenDPSReadout;
            Settings.Default.OpenGraphForSelf = OpenGraphForSelfAutomatically;
            Settings.Default.AnonymizePlayers = AnonymizePlayers;
            SettingsVM.SaveSettings();
            ClockFormat.Save();
            Settings.Default.Save();
            Console.WriteLine("Settings saved");
        }

        private void loadSettings()
        {
            HighlightDPS = Settings.Default.HighlightDPS;
            ShortenDPSReadout = Settings.Default.ShortenDPSReadout;
            OpenGraphForSelfAutomatically = Settings.Default.OpenGraphForSelf;
            AnonymizePlayers = Settings.Default.AnonymizePlayers;
            EnabledLineSeries val = EnabledLineSeries.All;
            DetailedDamageVisibleColumns val2 = DetailedDamageVisibleColumns.All;
            PSO2DamageTrackers hiddenTrackers = PSO2DamageTrackers.None;
            PSO2DamageTrackers splitTrackers = PSO2DamageTrackers.None;
            if (!Enum.TryParse<EnabledLineSeries>(Settings.Default.EnabledLineSeries, out val)) val = EnabledLineSeries.All;
            if (!Enum.TryParse<DetailedDamageVisibleColumns>(Settings.Default.DetailedDamageVisibleColumns, out val2)) val2 = DetailedDamageVisibleColumns.All;
            if (!Enum.TryParse<PSO2DamageTrackers>(Settings.Default.HiddenDamageTypes, out hiddenTrackers)) hiddenTrackers = PSO2DamageTrackers.None;
            if (!Enum.TryParse<PSO2DamageTrackers>(Settings.Default.SplitDamageTypes, out splitTrackers)) splitTrackers = PSO2DamageTrackers.None;
            if (!string.IsNullOrWhiteSpace(Settings.Default.BackgroundImagePath))
            {
                BackgroundImagePath = Settings.Default.BackgroundImagePath;
            }
            LineSeriesSettings = new EnabledLineSeriesSettingsVM(val);
            DetailedDamageVisibleSettings = new ColumnVisibilityVM(val2);
            DamageTypesSettings = new HiddenSplitTypesSettingsVM(splitTrackers, hiddenTrackers);
            DamageTypesSettings.HideSettingsChanged += DamageTypesSettings_HideSettingsChanged;
            DamageTypesSettings.SplitSettingsChanged += DamageTypesSettings_SplitSettingsChanged;
            LineSeriesSettings.SeriesEnabledStateChangedEvent += LineSeriesSettings_SeriesEnabledStateChangedEvent;
        }

        private void DamageTypesSettings_SplitSettingsChanged(object sender, EventArgs e)
        {
            var newSetting = DamageTypesSettings.GetSeparatedTrackersEnum(true);
            CurrentDamageParser.SetTrackersToSumInTotalDamage(newSetting);
        }

        private void DamageTypesSettings_HideSettingsChanged(object sender, EventArgs e)
        {
            var newSetting = DamageTypesSettings.GetHiddenTrackersEnum();
            CurrentDamageParser.SetTrackersToHide(newSetting);
        }

        private void LineSeriesSettings_SeriesEnabledStateChangedEvent(object sender, EventArgs e)
        {
            var newSetting = LineSeriesSettings.GetEnumValue();
            foreach (var playerTab in playerTabDict.Select(kvp => kvp.Value))
            {
                playerTab.SetEnabledLineSeries(newSetting);
            }
        }

        private void onSettingsChanged()
        {
            allPlayersTab?.RefreshState();
        }

        private void reselectDamageLogs()
        {
            if (selectDamageLogsPath(true))
            {
                MessageBox.Show("ApexParse must now be restarted.", "Restart Needed", MessageBoxButtons.OK);
                App.Current.Shutdown();
            }
        }

        private void initializeHotkeys()
        {
            if (!hotkeyUtil.RegisterHotkey(new HotkeyUtil.KeyContainer(Keys.R, true, true, (_) => { resetParser(); }))) //CTRL + SHIFT + R
            {
                warnHotkeysInitialized();
                return;
            }
            if (!hotkeyUtil.RegisterHotkey(new HotkeyUtil.KeyContainer(Keys.E, true, true, (_) => { saveSession(); }))) //CTRL + SHIFT + E
            {
                warnHotkeysInitialized();
                return;
            }
        }

        private void warnHotkeysInitialized()
        {
            MessageBox.Show("Unable to register hotkeys, is ApexParse already running?");
        }

        private void saveSession()
        {
            string report = CurrentDamageParser.GenerateSummary(true);
            DateTime logTime = CurrentDamageParser.LogStartTime;

            string dateString = UtilityMethods.ReplaceInvalidCharactersInPath($"{logTime:yyyy\\-MM\\-dd}");
            string timeString = UtilityMethods.ReplaceInvalidCharactersInPath($"{logTime:HH\\-mm\\-ss tt}");
            string destinationFolder = Path.Combine("Saved Sessions", dateString);
            string reportPath = Path.Combine(destinationFolder, $"ApexParse-{dateString}_{timeString}.txt");

            UtilityMethods.EnsureFolderExists(destinationFolder);
            File.WriteAllText(reportPath, report);
            if (SettingsVM.RenderWindow)
            {
                SaveToImagePath = Path.ChangeExtension(reportPath, "jpg");
                RenderNowRelay.Execute(); //lmao
            }

            StatusBarText += $" | Session Saved!";
            SavedSessions.Add(new SessionItemVM($"{dateString} - {timeString}", reportPath));
        }

        private void openSessionLogsFolder()
        {
            DateTime logTime = DateTime.Now;
            string dateString = UtilityMethods.ReplaceInvalidCharactersInPath($"{logTime:yyyy\\-MM\\-dd}");
            string destinationFolder = Path.Combine("Saved Sessions", dateString);
            if (!Directory.Exists(destinationFolder))
            {
                destinationFolder = "Saved Sessions";
                if (!Directory.Exists(destinationFolder))
                {
                    MessageBox.Show("No sessions saved. Try saving one first", "No sessions found", MessageBoxButtons.OK);
                    return;
                }
            }
            UtilityMethods.OpenWithDefaultProgram(destinationFolder);
        }

        private void AllPlayersTab_UserDoubleClickedEvent(object sender, UserDoubleClickedEventArgs e)
        {
            lock (tabManipulationLock)
            {
                if (playerTabDict.ContainsKey(e.DoubleClickedPlayer))
                {
                    SelectedTab = playerTabDict[e.DoubleClickedPlayer];
                }
            }
        }

        private void CurrentDamageParser_NewSessionStarted(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(resetAllTabs);
        }

        private void resetParser()
        {
            CurrentDamageParser.Reset();
            StatusBarText = "Waiting...";
            resetAllTabs();
        }

        private void resetAllTabs()
        {
            lock (tabManipulationLock)
            {
                AllTabs.Clear();
                allPlayersTab.ClearPlayers();
                playerTabDict.Clear();
                AllTabs.Add(allPlayersTab);
                selfPlayerTab = null;
            }
            SelectedTab = allPlayersTab;
        }

        private void Parser_UpdateTick(object sender, UpdateTickEventArgs e)
        {
            StatusBarText = $"{e.ElapsedTime.ToString(@"h\:mm\:ss")} - Total Damage : {CurrentDamageParser.TotalFriendlyDamage:#,##0} - {DamageParser.FormatDPSNumber(CurrentDamageParser.TotalFriendlyDPS)} DPS";
            App.Current.Dispatcher.Invoke(synchronizeTabState); //ew.
        }

        private void synchronizeTabState()
        {
            lock (tabManipulationLock)
            {
                if (selfPlayerTab == null && CurrentDamageParser.SelfPlayer != null)
                {
                    selfPlayerTab = new GraphPlayerTabVM(this, DetailedDamageVisibleSettings, CurrentDamageParser.SelfPlayer);
                    AllTabs.Insert(1, selfPlayerTab);
                    playerTabDict.Add(CurrentDamageParser.SelfPlayer, selfPlayerTab);
                    selfPlayerTab.SetEnabledLineSeries(LineSeriesSettings.GetEnumValue());

                    if (OpenGraphForSelfAutomatically)
                    {
                        SelectedTab = selfPlayerTab;
                    }
                }

                foreach (var player in CurrentDamageParser.Players) //add
                {
                    if (DamageParser.IsBlacklistedUsername(player.Name)) continue; //skip blacklisted
                    if (!playerTabDict.ContainsKey(player))
                    {
                        var newTab = new GraphPlayerTabVM(this, DetailedDamageVisibleSettings, player);
                        AllTabs.Add(newTab);
                        playerTabDict.Add(player, newTab);
                        newTab.SetEnabledLineSeries(LineSeriesSettings.GetEnumValue());
                    }
                }

                List<GraphPlayerTabVM> toRemove = new List<GraphPlayerTabVM>(); //remove
                foreach (var tab in AllTabs.Where(t => t is GraphPlayerTabVM).Cast<GraphPlayerTabVM>())
                {
                    if (!CurrentDamageParser.DoesPlayerIdExist(tab.Player.ID)) toRemove.Add(tab);
                }
                foreach (var rem in toRemove)
                {
                    AllTabs.Remove(rem);
                    playerTabDict.Remove(rem.Player);
                }
            }
        }
        
        private bool selectDamageLogsPath(bool initializeParser)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select your PSO2 damagelogs folder";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.DamageLogsPath = dialog.SelectedPath;
                    Settings.Default.Save();
                    if(initializeParser) initializeDamageParser(dialog.SelectedPath);
                    return true;
                }
            }

            return false;
        }

        void initializeDamageParser(string logPath)
        {
            CurrentDamageParser = new DamageParser(Settings.Default.DamageLogsPath);
            CurrentDamageParser.SetTrackersToSumInTotalDamage(DamageTypesSettings.GetSeparatedTrackersEnum(true));
            CurrentDamageParser.SetTrackersToHide(DamageTypesSettings.GetHiddenTrackersEnum());
            CurrentDamageParser.SetNameAnonimization(AnonymizePlayers);
            CurrentDamageParser.AutoEndSession = Settings.Default.AutoEndSession;
            CurrentDamageParser.AutoEndTimeout = Settings.Default.AutoEndSessionTimeout;
            CurrentDamageParser.UpdateTick += Parser_UpdateTick;
            CurrentDamageParser.NewSessionStarted += CurrentDamageParser_NewSessionStarted;
            CurrentDamageParser.AutoEndSessionEvent += CurrentDamageParser_AutoEndSessionEvent;
            
        }

        private void setBackgroundImage()
        {
            using (OpenFileDialog dialog = new OpenFileDialog() { Filter = "Valid Images|*.png;*.jpg;*.jpeg;*.gif;*.bmp", Title = "Select an image (GIFs will not be animated)" })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    BackgroundImagePath = dialog.FileName;
                }
                else
                {
                    if (MessageBox.Show("Clear background image?", "Clear Image", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        BackgroundImagePath = string.Empty;
                    }
                }
            }
        }

        void ChangeCustomAccentColor(System.Drawing.Color color)
        {
            Settings.Default.CustomAccentColor = color;
            AccentColorUtility.ReloadActiveColor();
        }

        private void setAccentColor()
        {
            using (ColorDialog selectColorDialog = new ColorDialog() { AllowFullOpen = true, AnyColor = true, SolidColorOnly = true })
            {
                if (selectColorDialog.ShowDialog() == DialogResult.OK)
                {
                    ChangeCustomAccentColor(selectColorDialog.Color);
                }
                else
                {
                    if (MessageBox.Show("Clear custom accent color?", "Clear Accent", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        ChangeCustomAccentColor(selectColorDialog.Color);
                    }
                }
            }
        }

        private void onShortenDPSChanged()
        {
            DamageParser.ShortenDPSValues = ShortenDPSReadout;
        }
    }
}
