using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse
{
    class PSO2DamageInstance
    {
        public DateTime Timestamp { get; private set; }

        public long InstanceId { get; private set; } = -1;

        public long SourceId { get; private set; }

        public string SourceName { get; private set; }

        public long TargetId { get; private set; }

        public string TargetName { get; private set; }

        public long AttackId { get; private set; }

        public long Damage { get; private set; }

        public bool IsJustAttack { get; private set; }

        public bool IsCrit { get; private set; }

        public bool IsMultiHit { get; private set; }

        public bool IsMisc { get; private set; }

        public bool IsMisc2 { get; private set; }

        public bool IsZanverseDamage { get; private set; }

        public bool IsAISDamage { get; private set; }

        public bool IsDarkBlastDamage { get; private set; }

        public bool IsHeroFinishDamage { get; private set; }

        public bool IsPhotonDamage { get; private set; }

        public bool IsRideroidDamage { get; private set; }
        
        public bool IsLaconiumDamage { get; private set; }

        public string AttackName { get; private set; }

        public TimeSpan RelativeTimestamp { get; private set; }

        public PSO2DamageInstance(string[] parts)
        {
            if (parts[0] == "timestamp")
            {
                BuildInvalidInstance();
                return;
            }
            try
            {
                Timestamp = FromUnixTime(long.Parse(parts[0]));
                InstanceId = long.Parse(parts[1]);
                SourceId = long.Parse(parts[2]);
                SourceName = parts[3];
                TargetId = long.Parse(parts[4]);
                TargetName = parts[5];
                AttackId = long.Parse(parts[6]);
                Damage = long.Parse(parts[7]);
                //i legit cannot believe bool.Parse doesn't consider 1/0 to be true/false.
                IsJustAttack = int.Parse(parts[8]) == 1;
                IsCrit = int.Parse(parts[9]) == 1;
                IsMultiHit = int.Parse(parts[10]) == 1;
                IsMisc = int.Parse(parts[11]) == 1;
                IsMisc2 = int.Parse(parts[12]) == 1;
            }
            catch
            {
                //Potentially not the best way to do this, but it should prevent crashes on improperly formatted lines
                BuildInvalidInstance();
                return;
            }

            IsZanverseDamage = AttackId == 2106601422;
            IsAISDamage = PSO2AttackNameHelper.IsAisAttack(AttackId);
            IsDarkBlastDamage = PSO2AttackNameHelper.IsDarkBlastAttack(AttackId);
            AttackName = PSO2AttackNameHelper.GetAttackName(AttackId);
            IsHeroFinishDamage = PSO2AttackNameHelper.IsHeroFinishAttack(AttackId);
            IsPhotonDamage = PSO2AttackNameHelper.IsPhotonAttack(AttackId);
            IsRideroidDamage = PSO2AttackNameHelper.IsRideroidAttack(AttackId);
            IsLaconiumDamage = PSO2AttackNameHelper.IsRideroidAttack(AttackId);
        }

        public void ReplaceZanverseName()
        {
            AttackName = SourceName;
        }

        private void BuildInvalidInstance()
        {
            AttackId = 0;
            TargetId = 0;
            SourceId = 0;
        }

        public void UpdateLogStartTime(DateTime logStartDate)
        {
            RelativeTimestamp = logStartDate == DateTime.MinValue ? TimeSpan.Zero : Timestamp - logStartDate;
        }

        private DateTime FromUnixTime(long unixTime)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime).ToLocalTime();
            return dtDateTime;
        }

        public override string ToString()
        {
            return $"sourceId = {SourceId}, targetId = {TargetId}, sourceName = {SourceName ?? "NULL"}, targetName = {TargetName ?? "NULL"}";
        }
    }
}
