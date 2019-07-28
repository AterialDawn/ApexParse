using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse.ViewModel
{
    //this class handles the settings interface for hidden/split damage types
    class HiddenSplitTypesSettingsVM : ViewModelBase
    {
        bool _splitAis;
        public bool SplitAIS
        {
            get { return _splitAis; }
            set { CallerSetProperty(ref _splitAis, value); onSplitChanged(); }
        }

        bool _hideAis;
        public bool HideAIS
        {
            get { return _hideAis; }
            set { CallerSetProperty(ref _hideAis, value); onHideChanged(); }
        }

        bool _splitDb;
        public bool SplitDB
        {
            get { return _splitDb; }
            set { CallerSetProperty(ref _splitDb, value); onSplitChanged(); }
        }

        bool _hideDb;
        public bool HideDB
        {
            get { return _hideDb; }
            set { CallerSetProperty(ref _hideDb, value); onHideChanged(); }
        }

        bool _splitHtf;
        public bool SplitHTF
        {
            get { return _splitHtf; }
            set { CallerSetProperty(ref _splitHtf, value); onSplitChanged(); }
        }

        bool _hideHtf;
        public bool HideHTF
        {
            get { return _hideHtf; }
            set { CallerSetProperty(ref _hideHtf, value); onHideChanged(); }
        }

        bool _splitLsw;
        public bool SplitLSW
        {
            get { return _splitLsw; }
            set { CallerSetProperty(ref _splitLsw, value); onSplitChanged(); }
        }

        bool _hideLsw;
        public bool HideLSW
        {
            get { return _hideLsw; }
            set { CallerSetProperty(ref _hideLsw, value); onHideChanged(); }
        }

        bool _splitPwp;
        public bool SplitPWP
        {
            get { return _splitPwp; }
            set { CallerSetProperty(ref _splitPwp, value); onSplitChanged(); }
        }

        bool _hidePwp;
        public bool HidePWP
        {
            get { return _hidePwp; }
            set { CallerSetProperty(ref _hidePwp, value); onHideChanged(); }
        }

        bool _splitRide;
        public bool SplitRide
        {
            get { return _splitRide; }
            set { CallerSetProperty(ref _splitRide, value); onSplitChanged(); }
        }

        bool _hideRide;
        public bool HideRide
        {
            get { return _hideRide; }
            set { CallerSetProperty(ref _hideRide, value); onHideChanged(); }
        }

        bool _splitZanverse;
        public bool SplitZanverse
        {
            get { return _splitZanverse; }
            set { CallerSetProperty(ref _splitZanverse, value); onSplitChanged(); }
        }

        bool _hideZanverse;
        public bool HideZanverse
        {
            get { return _hideZanverse; }
            set { CallerSetProperty(ref _hideZanverse, value); onHideChanged(); }
        }

        public event EventHandler SplitSettingsChanged;
        public event EventHandler HideSettingsChanged;

        public HiddenSplitTypesSettingsVM(PSO2DamageTrackers separatedTrackers, PSO2DamageTrackers hiddenTrackers)
        {
            SplitAIS = separatedTrackers.HasFlag(PSO2DamageTrackers.AIS);
            SplitDB = separatedTrackers.HasFlag(PSO2DamageTrackers.DarkBlast);
            SplitHTF = separatedTrackers.HasFlag(PSO2DamageTrackers.HTF);
            SplitPWP = separatedTrackers.HasFlag(PSO2DamageTrackers.PWP);
            SplitLSW = separatedTrackers.HasFlag(PSO2DamageTrackers.LSW);
            SplitRide = separatedTrackers.HasFlag(PSO2DamageTrackers.Ride);
            SplitZanverse = separatedTrackers.HasFlag(PSO2DamageTrackers.Zanverse);

            HideAIS = hiddenTrackers.HasFlag(PSO2DamageTrackers.AIS);
            HideDB = hiddenTrackers.HasFlag(PSO2DamageTrackers.DarkBlast);
            HideHTF = hiddenTrackers.HasFlag(PSO2DamageTrackers.HTF);
            HidePWP = hiddenTrackers.HasFlag(PSO2DamageTrackers.PWP);
            HideLSW = hiddenTrackers.HasFlag(PSO2DamageTrackers.LSW);
            HideRide = hiddenTrackers.HasFlag(PSO2DamageTrackers.Ride);
            HideZanverse = hiddenTrackers.HasFlag(PSO2DamageTrackers.Zanverse);
        }

        public PSO2DamageTrackers GetSeparatedTrackersEnum(bool passToParser)
        {
            PSO2DamageTrackers retVal = new PSO2DamageTrackers();
            retVal |= AddFlagInvert(SplitAIS, PSO2DamageTrackers.AIS, passToParser);
            retVal |= AddFlagInvert(SplitDB, PSO2DamageTrackers.DarkBlast, passToParser);
            retVal |= AddFlagInvert(SplitHTF, PSO2DamageTrackers.HTF, passToParser);
            retVal |= AddFlagInvert(SplitPWP, PSO2DamageTrackers.PWP, passToParser);
            retVal |= AddFlagInvert(SplitLSW, PSO2DamageTrackers.LSW, passToParser);
            retVal |= AddFlagInvert(SplitRide, PSO2DamageTrackers.Ride, passToParser);
            retVal |= AddFlagInvert(SplitZanverse, PSO2DamageTrackers.Zanverse, passToParser);
            if (passToParser)
            {
                retVal |= PSO2DamageTrackers.Basic; //parser takes this as a bitfield of trackers to COMBINE, since we wanna split, we invert to indicate that we wanna separate this one
            }
            return retVal;
        }

        public PSO2DamageTrackers GetHiddenTrackersEnum()
        {
            PSO2DamageTrackers retVal = new PSO2DamageTrackers();
            retVal |= AddFlag(HideAIS, PSO2DamageTrackers.AIS);
            retVal |= AddFlag(HideDB, PSO2DamageTrackers.DarkBlast);
            retVal |= AddFlag(HideHTF, PSO2DamageTrackers.HTF);
            retVal |= AddFlag(HidePWP, PSO2DamageTrackers.PWP);
            retVal |= AddFlag(HideLSW, PSO2DamageTrackers.LSW);
            retVal |= AddFlag(HideRide, PSO2DamageTrackers.Ride);
            retVal |= AddFlag(HideZanverse, PSO2DamageTrackers.Zanverse);
            return retVal;
        }

        void onHideChanged()
        {
            HideSettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        void onSplitChanged()
        {
            SplitSettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        PSO2DamageTrackers AddFlag(bool value, PSO2DamageTrackers flag)
        {
            if (value) return flag;
            return 0;
        }

        PSO2DamageTrackers AddFlagInvert(bool value, PSO2DamageTrackers flag, bool invert)
        {
            bool passedCheck = false;
            if (value) passedCheck = true;
            if (invert) passedCheck = !passedCheck;
            if (passedCheck) return flag;
            return 0;
        }
    }
}
