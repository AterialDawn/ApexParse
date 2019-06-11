using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse.ViewModel
{
    class DetailedAttackInfoVM : ViewModelBase
    {
        public long TotalDamage { get { return attackInfo.TotalDamage; } }

        string _attackName;
        public string AttackName
        {
            get { return _attackName; }
            set { CallerSetProperty(ref _attackName, value); }
        }

        string _attackCount;
        public string AttackCount
        {
            get { return _attackCount; }
            set { CallerSetProperty(ref _attackCount, value); }
        }

        string _attackTotalDamage;
        public string AttackTotalDamage
        {
            get { return _attackTotalDamage; }
            set { CallerSetProperty(ref _attackTotalDamage, value); }
        }

        string _attackJaPercent;
        public string AttackJAPercent
        {
            get { return _attackJaPercent; }
            set { CallerSetProperty(ref _attackJaPercent, value); }
        }

        string _attackCritPercent;
        public string AttackCritPercent
        {
            get { return _attackCritPercent; }
            set { CallerSetProperty(ref _attackCritPercent, value); }
        }

        string _attackMinDamage;
        public string AttackMinDamage
        {
            get { return _attackMinDamage; }
            set { CallerSetProperty(ref _attackMinDamage, value); }
        }

        string _attackAverageDamage;
        public string AttackAverageDamage
        {
            get { return _attackAverageDamage; }
            set { CallerSetProperty(ref _attackAverageDamage, value); }
        }

        string _attackMaxDamage;
        public string AttackMaxDamage
        {
            get { return _attackMaxDamage; }
            set { CallerSetProperty(ref _attackMaxDamage, value); }
        }

        GraphPlayerTabVM parent;
        PSO2Player.AttackInfo attackInfo;
        public DetailedAttackInfoVM(GraphPlayerTabVM parentVM, PSO2Player.AttackInfo attackInfo)
        {
            this.parent = parentVM;
            this.attackInfo = attackInfo;

            Update();
        }

        public void Update()
        {
            AttackName = attackInfo.Name;
            AttackCount = attackInfo.TotalHits.ToString();
            AttackTotalDamage = attackInfo.TotalDamage.ToString("#,##0");
            if (attackInfo.IsJustAttackValid)
            {
                AttackJAPercent = attackInfo.JustAttackPercent.ToString("0.00");
            }
            else
            {
                AttackJAPercent = "N/A";
            }
            AttackCritPercent = attackInfo.CritPercent.ToString("0.00");
            AttackMinDamage = attackInfo.MinDamage.ToString("#,##0");
            AttackAverageDamage = attackInfo.AverageDamage.ToString("#,##0");
            AttackMaxDamage = attackInfo.MaxDamage.ToString("#,##0");
        }
    }
}
