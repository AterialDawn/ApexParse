using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ApexParse.ViewModel
{
    /// <summary>
    /// very good name
    /// Represents a single player in the AllPlayersTabViewModel
    /// </summary>
    class AllPlayersTabPlayerVM : ViewModelBase
    {
        public PSO2Player AssociatedPlayer { get; private set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { CallerSetProperty(ref _name, value); }
        }

        private string _totalDamagePercent;
        public string TotalDamagePercent
        {
            get { return _totalDamagePercent; }
            set { CallerSetProperty(ref _totalDamagePercent, value); }
        }

        private string _totalDamageDealt;
        public string TotalDamageDealt
        {
            get { return _totalDamageDealt; }
            set { CallerSetProperty(ref _totalDamageDealt, value); }
        }

        private string _totalDamageTaken;
        public string TotalDamageTaken
        {
            get { return _totalDamageTaken; }
            set { CallerSetProperty(ref _totalDamageTaken, value); }
        }

        private string _totalDPS;
        public string TotalDPS
        {
            get { return _totalDPS; }
            set { CallerSetProperty(ref _totalDPS, value); }
        }

        private string _justAttackPercent;
        public string JustAttackPercent
        {
            get { return _justAttackPercent; }
            set { CallerSetProperty(ref _justAttackPercent, value); }
        }

        private string _critPercent;
        public string CritPercent
        {
            get { return _critPercent; }
            set { CallerSetProperty(ref _critPercent, value); }
        }

        private string _maxHitInfo;
        public string MaxHitInfo
        {
            get { return _maxHitInfo; }
            set { CallerSetProperty(ref _maxHitInfo, value); }
        }

        private LinearGradientBrush _backgroundBrush;
        public LinearGradientBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { CallerSetProperty(ref _backgroundBrush, value); }
        }

        private Color _highlightColor = Color.FromArgb(255, 100, 89, 223);
        public Color HighlightColor
        {
            get { return _highlightColor; }
            set { CallerSetProperty(ref _highlightColor, value); }
        }

        public long TotalDamage { get; private set; }

        private DamageParser parser;
        private MainWindowViewModel parent;

        public AllPlayersTabPlayerVM(MainWindowViewModel parentVM, PSO2Player player)
        {
            AssociatedPlayer = player;
            parent = parentVM;
            parser = parent.CurrentDamageParser;
            BackgroundBrush = new LinearGradientBrush();
            BackgroundBrush.StartPoint = new System.Windows.Point(0, 0);
            BackgroundBrush.EndPoint = new System.Windows.Point(1, 1);
            ParserUpdate();

            loadHighlightColor();
        }

        /// <summary>
        /// Called by owner VM to indicate that the parser has ticked
        /// </summary>
        public void ParserUpdate()
        {
            Name = AssociatedPlayer.Name;
            double totalDamagePercent = AssociatedPlayer.RelativeDPS;
            TotalDamagePercent = (totalDamagePercent * 100.0).ToString("0.00");
            TotalDamageDealt = $"{AssociatedPlayer.FilteredDamage.TotalDamage:#,##0}";
            TotalDamageTaken = $"{AssociatedPlayer.DamageTaken.TotalDamage:#,##0}";
            TotalDPS = $"{DamageParser.FormatDPSNumber(AssociatedPlayer.FilteredDamage.TotalDPS)}";
            JustAttackPercent = $"{AssociatedPlayer.FilteredDamage.JustAttackPercent:0.00}";
            CritPercent = $"{AssociatedPlayer.FilteredDamage.CritPercent:0.00}";
            MaxHitInfo = $"{AssociatedPlayer.MaxHit:#,##0} {AssociatedPlayer.MaxHitName}";

            TotalDamage = AssociatedPlayer.FilteredDamage.TotalDamage;
            
            if (totalDamagePercent > 0)
            {
                var scaledDamagePercent = totalDamagePercent;
                if (parser.HighestDpsPlayer != null)
                {
                    double highestPlayerDamagePercent = parser.HighestDpsPlayer.RelativeDPS;
                    scaledDamagePercent /= highestPlayerDamagePercent;
                }
                if (parent.HighlightDPS)
                {
                    BackgroundBrush.GradientStops.Clear();
                    BackgroundBrush.GradientStops.Add(new GradientStop(HighlightColor, 0));
                    BackgroundBrush.GradientStops.Add(new GradientStop(HighlightColor, scaledDamagePercent));
                    BackgroundBrush.GradientStops.Add(new GradientStop(Colors.Transparent, scaledDamagePercent));
                    BackgroundBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
                }
                else if (BackgroundBrush.GradientStops.Count > 0) BackgroundBrush.GradientStops.Clear(); //no highlight dps, but we have leftover gradient stops, clear em all out.
            }
        }

        private void loadHighlightColor()
        {
            var detectedStyle = ThemeManager.DetectAppStyle();
            if (detectedStyle.Item2.Resources["AccentColor3"] is Color)
            {
                HighlightColor = (Color)detectedStyle.Item2.Resources["AccentColor3"];
            }
        }
    }
}
