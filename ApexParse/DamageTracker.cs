using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse
{
    class DamageTracker
    {
        public long TotalDamage { get; private set; }

        public long UpdateDamage { get; private set; }

        public float TotalDPS { get; private set; }

        public float UpdateDPS { get; private set; }

        public bool AreJACritPercentsValid { get { return critJaHelper.IsValid; } }

        public float JustAttackPercent { get { return critJaHelper.JAPercent; } }

        public float CritPercent { get { return critJaHelper.CritPercent; } }

        public string Name { get; private set; }

        private List<PSO2DamageInstance> _damageInstances = new List<PSO2DamageInstance>();
        public IReadOnlyList<PSO2DamageInstance> RelevantDamageInstances { get { return _damageInstances; } }

        private TimeSpan updateFrequency;
        private TimeSpan historyDuration;
        private int updateTicks = 1;
        private Queue<PSO2DamageInstance> instanceHistory = new Queue<PSO2DamageInstance>();
        private JA_CritHelper critJaHelper = new JA_CritHelper();

        public DamageTracker(TimeSpan frequency, TimeSpan historyLength, string name)
        {
            updateFrequency = frequency;
            historyDuration = historyLength;
            Name = name;
        }

        public void UpdateReset()
        {
            UpdateDamage = 0;
            UpdateDPS = 0;
            updateTicks++;
        }

        public void Update(TimeSpan clockTime)
        {
            TotalDPS = (float)(TotalDamage / (updateFrequency.TotalSeconds * updateTicks));
            CheckHistory(clockTime); //check for 0 below
            if (instanceHistory.Count == 0)
            {
                UpdateDPS = 0;
            }
            else
            {
                CheckHistory(clockTime);
                float averagedSum = 0;
                foreach (var inst in instanceHistory)
                {
                    float weight = (float)((clockTime - inst.RelativeTimestamp).TotalSeconds / historyDuration.TotalSeconds);
                    averagedSum += inst.Damage * (1f - weight);
                }
                UpdateDPS = averagedSum / (float)instanceHistory.Count; // total damage divided by instances = dps over historyDuration
            }
        }

        private void CheckHistory(TimeSpan clockTime)
        {
            for (;;)
            {
                if (instanceHistory.Count == 0) return;
                PSO2DamageInstance curInstance = instanceHistory.Peek();
                if (clockTime - curInstance.RelativeTimestamp > historyDuration)
                {
                    instanceHistory.Dequeue(); //get rid of this, its too old now
                }
                else
                {
                    break;
                }
            }
        }

        public void AddDamage(PSO2DamageInstance instance)
        {
            TotalDamage += instance.Damage;
            UpdateDamage += instance.Damage;
            instanceHistory.Enqueue(instance);
            critJaHelper.AddDamage(instance);
            _damageInstances.Add(instance);
        }
    }
}
