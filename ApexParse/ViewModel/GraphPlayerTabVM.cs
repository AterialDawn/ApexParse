using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AppSettings = ApexParse.Properties.Settings;

namespace ApexParse.ViewModel
{
    class GraphPlayerTabVM : ViewModelBase
    {
        const int DPSAXIS = 0;
        const int TOTALDAMAGEAXIS = 1;
        const int UPDATEAXIS = 2;
        const int DAMAGETAKENAXIS = 3;
        const double LINESMOOTH = 0;

        public PSO2Player Player { get { return player; } }

        public SettingsViewModel Settings { get { return parent.SettingsVM; } }

        ObservableCollection<DetailedAttackInfoVM> _damageInfos = new ObservableCollection<DetailedAttackInfoVM>();

        Dictionary<string, DetailedAttackInfoVM> detailedVMDict = new Dictionary<string, DetailedAttackInfoVM>();

        private string _tabName;
        public string TabName
        {
            get { return _tabName; }
            set { CallerSetProperty(ref _tabName, value); }
        }

        private SeriesCollection _chartSeries;
        public SeriesCollection ChartSeries
        {
            get { return _chartSeries; }
            set { CallerSetProperty(ref _chartSeries, value); }
        }

        //this is nasty
        private double _damageTakenScale = 1;
        public double DamageTakenScale
        {
            get { return _damageTakenScale; }
            set { CallerSetProperty(ref _damageTakenScale, value); }
        }

        bool _areColumnsVisible;
        public bool AreColumnsVisible
        {
            get { return _areColumnsVisible; }
            set { CallerSetProperty(ref _areColumnsVisible, value); }
        }

        ColumnVisibilityVM _columnVisibility;
        public ColumnVisibilityVM ColumnVisibility
        {
            get { return _columnVisibility; }
            set { CallerSetProperty(ref _columnVisibility, value); }
        }

        ListCollectionView _damageInfosView;
        public ListCollectionView DamageInfosView
        {
            get { return _damageInfosView; }
            set { CallerSetProperty(ref _damageInfosView, value); }
        }

        private ChartValues<ObservablePoint> totalDamageValues = new ChartValues<ObservablePoint>();
        private ChartValues<ObservablePoint> totalDPSValues = new ChartValues<ObservablePoint>();
        private ChartValues<ObservablePoint> totalDamageTakenValues = new ChartValues<ObservablePoint>();
        private ChartValues<ObservablePoint> updateDPSValues = new ChartValues<ObservablePoint>();
        private ChartValues<ObservablePoint> averageMPAValues = new ChartValues<ObservablePoint>();
        private LineSeries totalDamageSeries;
        private LineSeries totalDPSSeries;
        private LineSeries totalDamageTakenSeries;
        private LineSeries updateDPSSeries;
        private LineSeries averageMPASeries;

        private Func<ChartPoint, string> pointFormatter;
        private Func<ChartPoint, string> itemLabelFormatter;

        private DamageParser parser;
        private PSO2Player player;
        private MainWindowViewModel parent;
        private EnabledLineSeries enabledSeries = EnabledLineSeries.All;

        public GraphPlayerTabVM(MainWindowViewModel parentVM, ColumnVisibilityVM columnVisVM, PSO2Player associatedPlayer)
        {
            parent = parentVM;
            parser = parentVM.CurrentDamageParser;
            player = associatedPlayer;
            TabName = associatedPlayer.Name;
            ColumnVisibility = columnVisVM;
            AreColumnsVisible = true;

            itemLabelFormatter = (c) => c.Y.ToString("0.##");
            pointFormatter = (c) => TimeSpan.FromSeconds(c.X).ToString();

            totalDamageValues.Add(new ObservablePoint(0, 0));
            totalDPSValues.Add(new ObservablePoint(0, 0));
            totalDamageTakenValues.Add(new ObservablePoint(0, 0));
            updateDPSValues.Add(new ObservablePoint(0, 0));
            averageMPAValues.Add(new ObservablePoint(0, 0));

            DamageInfosView = createViewSource();

            BuildChartSeries();

            parser.UpdateTick += Parser_UpdateTick;
        }

        public void SetEnabledLineSeries(EnabledLineSeries newSeries)
        {
            enabledSeries = newSeries;

            setSeriesVisibility(totalDamageSeries, EnabledLineSeries.TotalDamageDealt);
            setSeriesVisibility(totalDPSSeries, EnabledLineSeries.TotalDPS);
            setSeriesVisibility(totalDamageTakenSeries, EnabledLineSeries.TotalDamageTaken);
            setSeriesVisibility(updateDPSSeries, EnabledLineSeries.InstanceDPS);
            setSeriesVisibility(averageMPASeries, EnabledLineSeries.AverageMPADPS);
        }

        private void setSeriesVisibility(LineSeries series, EnabledLineSeries flag)
        {
            //Workaround for Visibility throwing NullReferenceException.
            //LiveCharts bug https://github.com/Live-Charts/Live-Charts/issues/693
            try
            {
                series.Visibility = enabledSeries.HasFlag(flag) ? Visibility.Visible : Visibility.Hidden;
            }
            catch (NullReferenceException) { }
        }

        public void Reset()
        {
            totalDamageTakenValues.Clear(); totalDamageTakenValues.Add(new ObservablePoint(0, 0));
            totalDPSValues.Clear(); totalDPSValues.Add(new ObservablePoint(0, 0));
            totalDamageTakenValues.Clear(); totalDamageTakenValues.Add(new ObservablePoint(0, 0));
            updateDPSValues.Clear(); updateDPSValues.Add(new ObservablePoint(0, 0));
            averageMPAValues.Clear(); averageMPAValues.Add(new ObservablePoint(0, 0));
            DamageTakenScale = 1;
        }

        private void Parser_UpdateTick(object sender, UpdateTickEventArgs e)
        {
            if (player == null || parser.NewDamageInstanceCount == 0)
            {
                return; //do nothing until damage starts, or damage actually occurs
            }
            //damage parser takes care of splitting up damages, so we can use player.FilteredDamage now
            long totalDamage = player.FilteredDamage.TotalDamage;
            float totalDPS = player.FilteredDamage.TotalDPS;
            float updateDPS = player.FilteredDamage.UpdateDPS;

            totalDamageValues.Add(new ObservablePoint(e.ElapsedTime.TotalSeconds, totalDamage));
            totalDPSValues.Add(new ObservablePoint(e.ElapsedTime.TotalSeconds, totalDPS));
            totalDamageTakenValues.Add(new ObservablePoint(e.ElapsedTime.TotalSeconds, player.DamageTaken.TotalDamage));
            updateDPSValues.Add(new ObservablePoint(e.ElapsedTime.TotalSeconds, player.FilteredDamage.UpdateDPS));
            averageMPAValues.Add(new ObservablePoint(e.ElapsedTime.TotalSeconds, parser.TotalFriendlyDPS));
            if (player.DamageTaken.TotalDamage > 0) DamageTakenScale = player.DamageTaken.TotalDamage * 1.1; //extra padding up top
            TabName = player.Name;

            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var curInfo in player.AttackInfoList)
                {
                    if (!detailedVMDict.ContainsKey(curInfo.Key))
                    {
                        var vmToAdd = new DetailedAttackInfoVM(this, curInfo.Value);
                        detailedVMDict.Add(curInfo.Key, vmToAdd);
                        _damageInfos.Add(vmToAdd);
                    }

                    detailedVMDict[curInfo.Key].Update();
                }

                DamageInfosView.Refresh();
            });
        }

        private ListCollectionView createViewSource()
        {
            var source = CollectionViewSource.GetDefaultView(_damageInfos) as ListCollectionView;
            source.CustomSort = new CustomSortAttackInfo();
            return source;
        }

        class CustomSortAttackInfo : IComparer
        {
            public int Compare(object x, object y)
            {
                var first = x as DetailedAttackInfoVM;
                var second = y as DetailedAttackInfoVM;

                int val = second.TotalDamage.CompareTo(first.TotalDamage);
                if (val == 0)
                {
                    return second.AttackName.CompareTo(first.AttackName);
                }
                else
                {
                    return val;
                }
            }
        }

        private void BuildChartSeries()
        {
            totalDPSSeries = new LineSeries
            {
                Values = totalDPSValues,
                StrokeThickness = AppSettings.Default.LineStrokeWidth,
                Fill = Brushes.Transparent,
                Stroke = Brushes.LightBlue,
                PointGeometrySize = 0,
                DataLabels = false,
                Title = "Total DPS",
                ScalesYAt = DPSAXIS,
                LabelPoint = itemLabelFormatter,
                LineSmoothness = LINESMOOTH
            };

            totalDamageSeries = new LineSeries
            {
                Values = totalDamageValues,
                StrokeThickness = AppSettings.Default.LineStrokeWidth,
                Fill = Brushes.Transparent,
                Stroke = Brushes.SkyBlue,
                PointGeometrySize = 0,
                DataLabels = false,
                Title = "Total Damage",
                ScalesYAt = TOTALDAMAGEAXIS,
                LabelPoint = itemLabelFormatter,
                LineSmoothness = LINESMOOTH
            };

            totalDamageTakenSeries = new LineSeries
            {
                Values = totalDamageTakenValues,
                StrokeThickness = AppSettings.Default.LineStrokeWidth,
                Fill = Brushes.Transparent,
                Stroke = Brushes.LightGreen,
                PointGeometrySize = 0,
                DataLabels = false,
                Title = "Total Damage Taken",
                ScalesYAt = DAMAGETAKENAXIS,
                LabelPoint = itemLabelFormatter,
                LineSmoothness = LINESMOOTH
            };

            updateDPSSeries = new LineSeries
            {
                Values = updateDPSValues,
                StrokeThickness = AppSettings.Default.LineStrokeWidth,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Turquoise,
                PointGeometrySize = 0,
                DataLabels = false,
                Title = "Instance DPS",
                ScalesYAt = UPDATEAXIS,
                LabelPoint = itemLabelFormatter,
                LineSmoothness = LINESMOOTH
            };

            averageMPASeries = new LineSeries
            {
                Values = averageMPAValues,
                StrokeThickness = AppSettings.Default.LineStrokeWidth,
                Fill = Brushes.Transparent,
                Stroke = Brushes.AliceBlue,
                PointGeometrySize = 0,
                DataLabels = false,
                Title = "Avg MPA DPS",
                ScalesYAt = DPSAXIS,
                LabelPoint = itemLabelFormatter,
                LineSmoothness = LINESMOOTH
            };
            ChartSeries = new SeriesCollection { totalDPSSeries, totalDamageSeries, totalDamageTakenSeries, updateDPSSeries, averageMPASeries };
        }
    }
}
