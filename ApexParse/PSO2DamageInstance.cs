using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse
{
    class PSO2DamageInstance
    {
        private static long[] AISAttackIDs = new long[] { 119505187, 79965782, 79965783, 79965784, 80047171, 434705298, 79964675, 1460054769, 4081218683, 3298256598, 2826401717 };
        private static long[] ElderDarkBlastIds = new long[] { 267911699, 262346668, 265285249, 264996390, 311089933, 3988916155, 265781051, 3141577094, 2289473436, 517914866, 517914869, 1117313539, 1611279117, 3283361988, 1117313602, 395090797, 2429416220, 1697271546, 1117313924 };
        private static long[] LoserDarkBlastIds = new long[] { 2743071591, 2743062721, 1783571383, 2928504078, 1783571188, 2849190450, 1223455602, 651603449, 2970658149, 2191939386, 2091027507, 4078260742 };
        private static long[] AppDarkBlastIds = new long[] { 3379639420, 3380458763, 3380192966, 3380628902, 3377229307, 3376960044, 3377051585, 3377849861, 855002982, 2326333456, 3725887474, 361825851, 3535795759, 781100939, 793625150, 1764406382, 3891439877, 2295506478, 1738105582, 37504833, 1891210633, 3617357696, 452272060 };
        private static long[] DoubleDarkBlastIds = new long[] { 2002943320, 2000047869, 2002496834, 1957174279, 1955884339, 4134333680, 4271466373, 305729398, 682884756, 4271465479, 3983075073, 4271465542, 3593316716, 483639921, 472092093 };

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
            IsAISDamage = AISAttackIDs.Contains(AttackId);
            IsDarkBlastDamage = ElderDarkBlastIds.Contains(AttackId) || LoserDarkBlastIds.Contains(AttackId) || AppDarkBlastIds.Contains(AttackId) || DoubleDarkBlastIds.Contains(AttackId);
            AttackName = PSO2AttackNameHelper.GetAttackName(AttackId);
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
