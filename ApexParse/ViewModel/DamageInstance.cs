using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse.ViewModel
{
    class DamageInstance : ViewModelBase
    {
        public DateTime Timestamp
        {
            get { return source.Timestamp; }
        }
        
        public string SourceName
        {
            get { return source.SourceName; }
        }
        
        public string TargetName
        {
            get { return source.TargetName; }
        }
        
        public long Damage
        {
            get { return source.Damage; }
        }
        
        public bool IsJustAttack
        {
            get { return source.IsJustAttack; }
        }

        private PSO2DamageInstance source;

        public DamageInstance(PSO2DamageInstance source)
        {
            this.source = source;
        }
    }
}
