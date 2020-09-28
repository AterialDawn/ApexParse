using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse
{
    class PSO2Player
    {
        public string Name { get; private set; }

        public bool IsSpecialPlayer { get; private set; } = false;

        public DamageView FilteredDamage { get; private set; }

        public DamageTracker BasicDamage { get; private set; }

        public DamageTracker ZanverseDamage { get; private set; }

        public DamageTracker DarkBlastDamage { get; private set; }

        public DamageTracker AISDamage { get; private set; }
        
        public DamageTracker PhotonDamage { get; private set; }

        public DamageTracker RideroidDamage { get; private set; }

        public DamageTracker LaconiumDamage { get; private set; }

        public DamageTracker StatusDamage { get; private set; }

        public DamageTracker HeroTimeFinishDamage { get; private set; }

        public DamageTracker DamageTaken { get; private set; }

        private Dictionary<string, AttackInfo> _attackInfoList = new Dictionary<string, AttackInfo>();
        public IReadOnlyDictionary<string, AttackInfo> AttackInfoList { get { return _attackInfoList; } }

        /// <summary>
        /// Relative dps of this player, range 0-1
        /// </summary>
        public double RelativeDPS { get; private set; } = 0;

        public long MaxHit { get; private set; } = 0;

        public string MaxHitName { get; private set; } = "";

        public long ID { get; private set; }

        private List<DamageTracker> allTrackers = new List<DamageTracker>();

        private List<PSO2DamageInstance> _damageInstances = new List<PSO2DamageInstance>();
        public IReadOnlyList<PSO2DamageInstance> DealtDamageInstances { get { return _damageInstances; } }

        private List<PSO2DamageInstance> _receivedDamageInstances = new List<PSO2DamageInstance>();
        public IReadOnlyList<PSO2DamageInstance> ReceivedDamageInstances { get { return _receivedDamageInstances; } }

        private PSO2DamageTrackers trackersToSum;
        private DamageParser parent;

        bool shouldAnonymizeName = true;
        public bool IsAlly
        {
            get
            {
                return IsAllyId(ID);
            }
        }

        public static bool IsAllyId(long id)
        {
            return id >= 10000000;
        }

        private string actualName;

        public PSO2Player(string playerName, long playerId, TimeSpan updateClock, TimeSpan instanceHistoryDuration, PSO2DamageTrackers trackersToCombine, DamageParser parentInst)
        {
            ID = playerId;
            Name = actualName = playerName;
            trackersToSum = trackersToCombine;
            FilteredDamage = new DamageView(trackersToCombine);
            parent = parentInst;
            parent.NameAnonimizationChangedEvent += Parent_NameAnonimizationChangedEvent;
            
            BasicDamage = new DamageTracker(updateClock, instanceHistoryDuration, "Basic"); FilteredDamage.RegisterTracker(BasicDamage, PSO2DamageTrackers.Basic);
            ZanverseDamage = new DamageTracker(updateClock, instanceHistoryDuration, "Zanverse"); FilteredDamage.RegisterTracker(ZanverseDamage, PSO2DamageTrackers.Zanverse);
            DarkBlastDamage = new DamageTracker(updateClock, instanceHistoryDuration, "DarkBlast"); FilteredDamage.RegisterTracker(DarkBlastDamage, PSO2DamageTrackers.DarkBlast);
            AISDamage = new DamageTracker(updateClock, instanceHistoryDuration, "AIS"); FilteredDamage.RegisterTracker(AISDamage, PSO2DamageTrackers.AIS);
            PhotonDamage = new DamageTracker(updateClock, instanceHistoryDuration, "PwP"); FilteredDamage.RegisterTracker(PhotonDamage, PSO2DamageTrackers.PWP);
            RideroidDamage = new DamageTracker(updateClock, instanceHistoryDuration, "Ride"); FilteredDamage.RegisterTracker(RideroidDamage, PSO2DamageTrackers.Ride);
            LaconiumDamage = new DamageTracker(updateClock, instanceHistoryDuration, "Lsw"); FilteredDamage.RegisterTracker(LaconiumDamage, PSO2DamageTrackers.LSW);
            HeroTimeFinishDamage = new DamageTracker(updateClock, instanceHistoryDuration, "HTF"); FilteredDamage.RegisterTracker(HeroTimeFinishDamage, PSO2DamageTrackers.HTF);
            StatusDamage = new DamageTracker(updateClock, instanceHistoryDuration, "Status"); FilteredDamage.RegisterTracker(StatusDamage, PSO2DamageTrackers.Burn);
            DamageTaken = new DamageTracker(updateClock, instanceHistoryDuration, "Taken");

            allTrackers.AddRange(new[] { BasicDamage, ZanverseDamage, DarkBlastDamage, AISDamage, DamageTaken, PhotonDamage, RideroidDamage, LaconiumDamage, HeroTimeFinishDamage, StatusDamage }); //to simplify calls below...
            Parent_NameAnonimizationChangedEvent(null, null);
        }

        public void SetSpecialPlayer(bool isSpecial, bool anonymizeName)
        {
            IsSpecialPlayer = isSpecial;
            shouldAnonymizeName = anonymizeName;
            if (isSpecial && !anonymizeName) Name = actualName;
            else updateName();
        }

        private void Parent_NameAnonimizationChangedEvent(object sender, EventArgs e)
        {
            updateName();
        }

        void updateName()
        {
            if (parent.AreNamesAnonimized)
            {
                if (ID == parent.SelfPlayerID)
                {
                    Name = "You";
                }
                else if (shouldAnonymizeName)
                {
                    Name = "-";
                }
                else
                {
                    Name = actualName;
                }
            }
            else
            {
                Name = actualName;
            }
        }

        /// <summary>
        /// Bitflags to include related tracker damage in TotalDamage
        /// </summary>
        /// <param name="trackers"></param>
        public void SetTrackersToIncludeInTotalDamage(PSO2DamageTrackers trackers)
        {
            trackersToSum = trackers;
            FilteredDamage.ChangeEnabledTrackers(trackers);
        }

        public void UpdateTick()
        {
            foreach (var tracker in allTrackers) tracker.UpdateReset();
        }

        public void RecalculateDPS(TimeSpan clockTime)
        {
            foreach (var tracker in allTrackers) tracker.Update(clockTime);
            foreach (var info in AttackInfoList) info.Value.Update(clockTime);
            FilteredDamage.Update();
        }

        public void UpdateRelativeDps(long totalMPADamage)
        {
            RelativeDPS = (double)FilteredDamage.TotalDamage / (double)totalMPADamage;
        }

        //specifically for Zanverse player, since it has an invalid ID, AddDamageInstance would ignore it
        public void AddZanverseDamageInstance(PSO2DamageInstance instance)
        {
            //instance.ReplaceZanverseName(); //due to adding AnonymizeDamage feature, names might get leaked via zanverse player, so just disable name replacement for now
            ZanverseDamage.AddDamage(instance);
            if (MaxHit < instance.Damage)
            {
                MaxHit = instance.Damage;
                MaxHitName = instance.AttackName;
            }
            _damageInstances.Add(instance);
        }

        public void AddDamageInstance(PSO2DamageInstance instance)
        {
            if (instance.TargetId == ID)
            {
                //we took damage from instance.SourceId
                _receivedDamageInstances.Add(instance);
                DamageTaken.AddDamage(instance);

                if (!_attackInfoList.ContainsKey("Damage Taken"))
                {
                    _attackInfoList.Add("Damage Taken", new AttackInfo("Damage Taken", 0));
                }
                _attackInfoList["Damage Taken"].AddDamageInstance(instance);
            }
            else if (instance.SourceId == ID)
            {
                //we dealt damage to instance.TargetId
                if (instance.IsAISDamage)
                {
                    AISDamage.AddDamage(instance);

                    if (trackersToSum.HasFlag(PSO2DamageTrackers.AIS)) AddAttackInfo(instance);
                }
                else if (instance.IsDarkBlastDamage)
                {
                    DarkBlastDamage.AddDamage(instance);

                    if (trackersToSum.HasFlag(PSO2DamageTrackers.DarkBlast)) AddAttackInfo(instance);
                }
                else if (instance.IsZanverseDamage)
                {
                    ZanverseDamage.AddDamage(instance);

                    if (trackersToSum.HasFlag(PSO2DamageTrackers.Zanverse)) AddAttackInfo(instance);
                }
                else if (instance.IsHeroFinishDamage)
                {
                    HeroTimeFinishDamage.AddDamage(instance);

                    if (trackersToSum.HasFlag(PSO2DamageTrackers.HTF)) AddAttackInfo(instance);
                }
                else if (instance.IsStatusDamage)
                {
                    StatusDamage.AddDamage(instance);

                    if (trackersToSum.HasFlag(PSO2DamageTrackers.Burn)) AddAttackInfo(instance);
                }
                else if (instance.IsPhotonDamage)
                {
                    PhotonDamage.AddDamage(instance);

                    if (trackersToSum.HasFlag(PSO2DamageTrackers.PWP)) AddAttackInfo(instance);
                }
                else if (instance.IsRideroidDamage)
                {
                    RideroidDamage.AddDamage(instance);

                    if (trackersToSum.HasFlag(PSO2DamageTrackers.Ride)) AddAttackInfo(instance);
                }
                else if (instance.IsLaconiumDamage)
                {
                    LaconiumDamage.AddDamage(instance);

                    if (trackersToSum.HasFlag(PSO2DamageTrackers.LSW)) AddAttackInfo(instance);
                }
                else
                {
                    BasicDamage.AddDamage(instance);

                    if (trackersToSum.HasFlag(PSO2DamageTrackers.Basic)) AddAttackInfo(instance);
                }
                
                _damageInstances.Add(instance);

                if (instance.IsZanverseDamage) //only update ZV max hit if ZV is not separated
                {
                    if (trackersToSum.HasFlag(PSO2DamageTrackers.Zanverse))
                    {
                        UpdateMaxHit(instance);
                    }
                }
                else
                {
                    UpdateMaxHit(instance);
                }
            }
        }

        void UpdateMaxHit(PSO2DamageInstance instance)
        {
            if (MaxHit < instance.Damage)
            {
                MaxHit = instance.Damage;
                MaxHitName = instance.AttackName;
            }
        }

        private void AddAttackInfo(PSO2DamageInstance instance)
        {
            if (!_attackInfoList.ContainsKey(instance.AttackName)) _attackInfoList.Add(instance.AttackName, new AttackInfo(instance.AttackName, instance.AttackId));
            var container = _attackInfoList[instance.AttackName];
            container.AddDamageInstance(instance);
        }

        //Moved here, as it's used in both the Parser report generator and the GraphPlayerTabVM
        public string GenerateDamageSummary()
        {
            StringBuilder sb = new StringBuilder($"[ {Name} - {RelativeDPS * 100f:0.00}% - {FilteredDamage.TotalDamage:#,##0} dmg ]");
            sb.AppendLine();
            Dictionary<string, AttackInfo> instanceContainer = new Dictionary<string, AttackInfo>();

            foreach (var damageInstance in FilteredDamage.GetFilteredDamageInstances())
            {
                if (!instanceContainer.ContainsKey(damageInstance.AttackName))
                {
                    instanceContainer.Add(damageInstance.AttackName, new AttackInfo(damageInstance.AttackName, damageInstance.AttackId));
                }

                var container = instanceContainer[damageInstance.AttackName];
                container.AddDamageInstance(damageInstance);
            }

            foreach (var container in instanceContainer.OrderByDescending(kvp => kvp.Value.TotalDamage))
            {
                container.Value.WriteSummaryToStringBuilder(sb, FilteredDamage.TotalDamage);
            }

            return sb.ToString();
        }

        public class AttackInfo
        {
            public string Name { get; private set; }
            public long TotalDamage { get; private set; }
            public float JustAttackPercent { get { return ja_critHelper.JAPercent; } }
            public float CritPercent { get { return ja_critHelper.CritPercent; } }
            public long TotalHits { get; private set; }
            public long MinDamage { get; private set; }
            public float AverageDamage { get { return TotalHits != 0 ? ((float)TotalDamage / (float)TotalHits) : 0; } }
            public float DPS { get; private set; } = 0;
            public long MaxDamage { get; private set; }
            public long AttackId { get; private set; }

            public bool IsJustAttackValid { get { return ja_critHelper.IsJAValid; } }

            JA_CritHelper ja_critHelper = new JA_CritHelper();

            internal AttackInfo(string attackName, long attackId)
            {
                Name = attackName;
                AttackId = attackId;
            }

            internal void AddDamageInstance(PSO2DamageInstance instance)
            {
                TotalDamage += instance.Damage;
                TotalHits++;
                if (MinDamage > 0)
                {
                    if (instance.Damage < MinDamage) MinDamage = instance.Damage;
                }
                else MinDamage = instance.Damage;
                if (MaxDamage > 0)
                {
                    if (instance.Damage > MaxDamage) MaxDamage = instance.Damage;
                }
                else MaxDamage = instance.Damage;

                ja_critHelper.AddDamage(instance);
            }

            public void Update(TimeSpan clockTime)
            {
                DPS = (float)(TotalDamage / clockTime.TotalSeconds);
            }

            internal void WriteSummaryToStringBuilder(StringBuilder sb, long totalPlayerDamage)
            {
                string totalDamagePercent;
                if (Name == "Damage Taken")
                {
                    totalDamagePercent = "   N/A";
                }
                else
                {
                    float percentOfTotalDamage = totalPlayerDamage == 0 ? 0f : (float)TotalDamage * 100f / (float)totalPlayerDamage;
                    totalDamagePercent = percentOfTotalDamage.ToString("00.00\\%");
                }
                
                sb.AppendLine($"{totalDamagePercent} | {Name} ({TotalDamage:#,##0} dmg)")
                    .AppendLine($"       |   JA : {(ja_critHelper.IsJAValid ? ($"{JustAttackPercent:00.00}%") : "N/A")} - Crit : {CritPercent:00.00}%")
                    .AppendLine($"       |   {TotalHits:#,##0} hits - {MinDamage:#,##0} min, {AverageDamage:#,##0.00} avg, {MaxDamage:#,##0} max");
            }
        }
    }
}
