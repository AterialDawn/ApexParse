using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ApexParse.ViewModel
{
    class ColumnVisibilityVM : ViewModelBase
    {
        Visibility _name;
        public Visibility Name
        {
            get { return _name; }
            set { CallerSetProperty(ref _name, value); }
        }

        Visibility _count;
        public Visibility Count
        {
            get { return _count; }
            set { CallerSetProperty(ref _count, value); }
        }

        Visibility _totalDamage;
        public Visibility TotalDamage
        {
            get { return _totalDamage; }
            set { CallerSetProperty(ref _totalDamage, value); }
        }

        Visibility _jaPercent;
        public Visibility JAPercent
        {
            get { return _jaPercent; }
            set { CallerSetProperty(ref _jaPercent, value); }
        }

        Visibility _critPercent;
        public Visibility CritPercent
        {
            get { return _critPercent; }
            set { CallerSetProperty(ref _critPercent, value); }
        }

        Visibility _minDamage;
        public Visibility MinDamage
        {
            get { return _minDamage; }
            set { CallerSetProperty(ref _minDamage, value); }
        }

        Visibility _averageDamage;
        public Visibility AverageDamage
        {
            get { return _averageDamage; }
            set { CallerSetProperty(ref _averageDamage, value); }
        }

        Visibility _maxDamage;
        public Visibility MaxDamage
        {
            get { return _maxDamage; }
            set { CallerSetProperty(ref _maxDamage, value); }
        }

        Visibility _dps;
        public Visibility DPS
        {
            get { return _dps; }
            set { CallerSetProperty(ref _dps, value); }
        }

        private DetailedDamageVisibleColumns visibilityEnum;

        public ColumnVisibilityVM(DetailedDamageVisibleColumns visEnum)
        {
            visibilityEnum = visEnum;
            Name = GetVisibility(DetailedDamageVisibleColumns.Name);
            Count = GetVisibility(DetailedDamageVisibleColumns.Count);
            TotalDamage = GetVisibility(DetailedDamageVisibleColumns.TotalDamage);
            JAPercent = GetVisibility(DetailedDamageVisibleColumns.JAPercent);
            CritPercent = GetVisibility(DetailedDamageVisibleColumns.CritPercent);
            MinDamage = GetVisibility(DetailedDamageVisibleColumns.MinDamage);
            AverageDamage = GetVisibility(DetailedDamageVisibleColumns.AverageDamage);
            MaxDamage = GetVisibility(DetailedDamageVisibleColumns.MaxDamage);
            DPS = GetVisibility(DetailedDamageVisibleColumns.DPS);
        }

        private Visibility GetVisibility(DetailedDamageVisibleColumns flag)
        {
            return visibilityEnum.HasFlag(flag) ? Visibility.Visible : Visibility.Collapsed;
        }

        public DetailedDamageVisibleColumns GetEnum()
        {
            DetailedDamageVisibleColumns retVal = new DetailedDamageVisibleColumns();
            retVal |= AddFlag(Name, DetailedDamageVisibleColumns.Name);
            retVal |= AddFlag(Count, DetailedDamageVisibleColumns.Count);
            retVal |= AddFlag(TotalDamage, DetailedDamageVisibleColumns.TotalDamage);
            retVal |= AddFlag(JAPercent, DetailedDamageVisibleColumns.JAPercent);
            retVal |= AddFlag(CritPercent, DetailedDamageVisibleColumns.CritPercent);
            retVal |= AddFlag(MinDamage, DetailedDamageVisibleColumns.MinDamage);
            retVal |= AddFlag(AverageDamage, DetailedDamageVisibleColumns.AverageDamage);
            retVal |= AddFlag(MaxDamage, DetailedDamageVisibleColumns.MaxDamage);
            retVal |= AddFlag(DPS, DetailedDamageVisibleColumns.DPS);
            return retVal;
        }

        DetailedDamageVisibleColumns AddFlag(Visibility value, DetailedDamageVisibleColumns flag)
        {
            if (value == Visibility.Visible) return flag;
            return 0;
        }
    }
}
