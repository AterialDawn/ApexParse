using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse
{
    class JA_CritHelper
    {
        public float JAPercent { get; private set; } = 0;
        public float CritPercent { get; private set; } = 0;

        public bool IsValid { get { return totalHitCount > 0; } }
        public bool IsJAValid { get { return totalJAHitCount > 0; } }

        private long jaHitCount = 0;
        private long critHitCount = 0;
        private long totalHitCount = 0;
        private long totalJAHitCount = 0;

        public void AddDamage(PSO2DamageInstance instance)
        {
            totalHitCount++;
            if (instance.IsCrit) critHitCount++;
            if (!PSO2AttackNameHelper.IsIgnoredAttackForJA(instance.AttackId))
            {
                if (instance.IsJustAttack) jaHitCount++;
                totalJAHitCount++;
				JAPercent = (float)jaHitCount * 100f / (float)totalJAHitCount;
            }
            
            CritPercent = (float)critHitCount * 100f / (float)totalHitCount;
        }
    }
}
