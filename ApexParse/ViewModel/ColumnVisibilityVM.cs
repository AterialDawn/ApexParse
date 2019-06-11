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

        private DetailedDamageVisibleColumns visibilityEnum;

        public ColumnVisibilityVM(DetailedDamageVisibleColumns visEnum)
        {
            visibilityEnum = visEnum;
            Name = HasFlag(DetailedDamageVisibleColumns.Name) ? Visibility.Visible : Visibility.Collapsed;
            Count = HasFlag(DetailedDamageVisibleColumns.Count) ? Visibility.Visible : Visibility.Collapsed;
            TotalDamage = HasFlag(DetailedDamageVisibleColumns.TotalDamage) ? Visibility.Visible : Visibility.Collapsed;
            JAPercent = HasFlag(DetailedDamageVisibleColumns.JAPercent) ? Visibility.Visible : Visibility.Collapsed;
            CritPercent = HasFlag(DetailedDamageVisibleColumns.CritPercent) ? Visibility.Visible : Visibility.Collapsed;
            MinDamage = HasFlag(DetailedDamageVisibleColumns.MinDamage) ? Visibility.Visible : Visibility.Collapsed;
            AverageDamage = HasFlag(DetailedDamageVisibleColumns.AverageDamage) ? Visibility.Visible : Visibility.Collapsed;
            MaxDamage = HasFlag(DetailedDamageVisibleColumns.MaxDamage) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool HasFlag(DetailedDamageVisibleColumns flag)
        {
            return visibilityEnum.HasFlag(flag);
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
            return retVal;
        }

        DetailedDamageVisibleColumns AddFlag(Visibility value, DetailedDamageVisibleColumns flag)
        {
            if (value == Visibility.Visible) return flag;
            return 0;
        }
    }
}
