using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse
{
    class DamageView
    {
        public long TotalDamage { get; private set; } = 0;
        public long UpdateDamage { get; private set; } = 0;
        public float TotalDPS { get; private set; } = 0;
        public float UpdateDPS { get; private set; } = 0;
        public float JustAttackPercent { get; private set; } = 0;
        public float CritPercent { get; private set; } = 0;

        List<TrackerContainer> allTrackers = new List<TrackerContainer>();
        PSO2DamageTrackers enabledTrackers;

        public DamageView(PSO2DamageTrackers activeTrackers)
        {
            enabledTrackers = activeTrackers;
        }

        public void ChangeEnabledTrackers(PSO2DamageTrackers activeTrackers)
        {
            enabledTrackers = activeTrackers;
            Update();
        }

        public void RegisterTracker(DamageTracker tracker, PSO2DamageTrackers flag)
        {
            allTrackers.Add(new TrackerContainer(tracker, flag));
        }

        public IEnumerable<PSO2DamageInstance> GetFilteredDamageInstances()
        {
            foreach (var damageInstance in allTrackers.Where(t => enabledTrackers.HasFlag(t.AssociatedFlag)).Select(t => t.AssociatedTracker).SelectMany(t => t.RelevantDamageInstances).OrderBy(d => d.Timestamp))
            {
                yield return damageInstance;
            }
        }

        public void Update()
        {
            TotalDamage = 0; UpdateDamage = 0; TotalDPS = 0; UpdateDPS = 0; CritPercent = 0; JustAttackPercent = 0;
            int percentDivisor = 0;
            foreach (var tracker in allTrackers.Where(t => enabledTrackers.HasFlag(t.AssociatedFlag)).Select(t => t.AssociatedTracker))
            {
                TotalDamage += tracker.TotalDamage;
                UpdateDamage += tracker.UpdateDamage;
                TotalDPS += tracker.TotalDPS;
                UpdateDPS += tracker.UpdateDPS;
                
                if (tracker.AreJACritPercentsValid)
                {
                    CritPercent += tracker.CritPercent;
                    JustAttackPercent += tracker.JustAttackPercent;
                    percentDivisor++;
                }
            }

            if (percentDivisor > 0)
            {
                CritPercent /= (float)percentDivisor;
                JustAttackPercent /= (float)percentDivisor;
            }
        }

        class TrackerContainer
        {
            public DamageTracker AssociatedTracker { get; private set; }
            public PSO2DamageTrackers AssociatedFlag { get; private set; }

            public TrackerContainer(DamageTracker tracker, PSO2DamageTrackers flag)
            {
                AssociatedTracker = tracker;
                AssociatedFlag = flag;
            }
        }
    }
}
