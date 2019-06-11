using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse.ViewModel
{
    class EnabledLineSeriesSettingsVM : ViewModelBase
    {
        bool _totalDPSEnabled;
        public bool TotalDPSEnabled
        {
            get { return _totalDPSEnabled; }
            set { CallerSetProperty(ref _totalDPSEnabled, value); notifyEnabledStateChanged(); }
        }

        bool _totalDamageEnabled;
        public bool TotalDamageEnabled
        {
            get { return _totalDamageEnabled; }
            set { CallerSetProperty(ref _totalDamageEnabled, value); notifyEnabledStateChanged(); }
        }

        bool _takenDamageEnabled;
        public bool TakenDamageEnabled
        {
            get { return _takenDamageEnabled; }
            set { CallerSetProperty(ref _takenDamageEnabled, value); notifyEnabledStateChanged(); }
        }

        bool _instanceDPSEnabled;
        public bool InstanceDPSEnabled
        {
            get { return _instanceDPSEnabled; }
            set { CallerSetProperty(ref _instanceDPSEnabled, value); notifyEnabledStateChanged(); }
        }

        bool _mpaAverageEnabled;
        public bool MPAAverageEnabled
        {
            get { return _mpaAverageEnabled; }
            set { CallerSetProperty(ref _mpaAverageEnabled, value); notifyEnabledStateChanged(); }
        }

        public event EventHandler SeriesEnabledStateChangedEvent;

        public EnabledLineSeriesSettingsVM(EnabledLineSeries series)
        {
            TotalDPSEnabled = series.HasFlag(EnabledLineSeries.TotalDPS);
            TotalDamageEnabled = series.HasFlag(EnabledLineSeries.TotalDamageDealt);
            TakenDamageEnabled = series.HasFlag(EnabledLineSeries.TotalDamageTaken);
            InstanceDPSEnabled = series.HasFlag(EnabledLineSeries.InstanceDPS);
            MPAAverageEnabled = series.HasFlag(EnabledLineSeries.AverageMPADPS);
        }

        public EnabledLineSeries GetEnumValue()
        {
            EnabledLineSeries retVal = new EnabledLineSeries();
            retVal |= AddFlag(TotalDPSEnabled, EnabledLineSeries.TotalDPS);
            retVal |= AddFlag(TotalDamageEnabled, EnabledLineSeries.TotalDamageDealt);
            retVal |= AddFlag(TakenDamageEnabled, EnabledLineSeries.TotalDamageTaken);
            retVal |= AddFlag(InstanceDPSEnabled, EnabledLineSeries.InstanceDPS);
            retVal |= AddFlag(MPAAverageEnabled, EnabledLineSeries.AverageMPADPS);
            return retVal;
        }

        private void notifyEnabledStateChanged()
        {
            SeriesEnabledStateChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        EnabledLineSeries AddFlag(bool value, EnabledLineSeries flag)
        {
            if (value) return flag;
            return 0;
        }
    }
}
