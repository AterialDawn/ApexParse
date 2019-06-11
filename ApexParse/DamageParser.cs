using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace ApexParse
{
    partial class DamageParser
    {
        public static bool ShortenDPSValues { get; set; } = false;

        private Dictionary<long, PSO2Player> _players = new Dictionary<long, PSO2Player>();
        public IReadOnlyDictionary<long, PSO2Player> Players { get { return _players; } }

        public event EventHandler NewSessionStarted;

        public long TotalFriendlyDamage { get; private set; } = 0;
        public double TotalFriendlyDPS { get; private set; } = 0;

        public PSO2Player SelfPlayer { get { return _players.ContainsKey(selfPlayerId) ? _players[selfPlayerId] : null; } }
        public PSO2Player HighestDpsPlayer { get; private set; } = null;
        public PSO2Player ZanversePlayer { get; private set; }
        public TimeSpan InstanceUpdateHistoryDuration { get; private set; } = TimeSpan.FromSeconds(5); //5 second history on instance updates
        public DateTime LogStartTime { get { return hasLogStartTime ? timeLogStarted : DateTime.MinValue; } }
        public int NewDamageInstanceCount { get; private set; } = 0;
        public bool ParsingActive { get { return hasLogStartTime; } }
        public bool IsZanverseSplit { get { return !trackersToSum.HasFlag(PSO2DamageTrackers.Zanverse); } }

        public event EventHandler<UpdateTickEventArgs> UpdateTick;

        private string monitoredFolder;
        private StreamReader logStream = null;
        private Timer updateTimer = null;
        private DateTime timeLastUpdateInvoked = DateTime.MinValue;
        private TimeSpan timeUntilSendManualUpdate; //after this time elapses, send out another UpdateTick containing the final values. avoids stale data if we got damage instances that dont trigger the UpdateFrequency timer
        private object updateLock = new object();
        private long selfPlayerId = -1;
        private TimeSpan updateClock = TimeSpan.Zero;
        DateTime timeLogStarted = DateTime.MinValue;
        TimeSpan lastUpdateTime = TimeSpan.Zero;
        PSO2DamageInstance lastDamageInstance = null;
        bool hasLogStartTime = false;
        bool resetParserOnNewInstance = false;
        int damageInstancesQueued = 0;
        PSO2DamageTrackers trackersToSum = PSO2DamageTrackers.All;
        private long flag = 0x6c616972657461; //the string 'aterial' as a long.

        public static bool IsBlacklistedUsername(string username)
        {
            return username == "YOU" || username == "unknown" || username == "Unknown";
        }

        public static string FormatDPSNumber(double dps)
        {
            if (ShortenDPSValues)
            {
                if (dps >= 100000000)
                    return (dps / 1000000.0).ToString("#,0") + "M";
                if (dps >= 1000000)
                    return (dps / 1000000.0).ToString("0.0") + "M";
                if (dps >= 100000)
                    return (dps / 1000.0).ToString("#,0") + "K";
                if (dps >= 1000)
                    return (dps / 1000.0).ToString("0.0") + "K";
                return dps.ToString("#,0");
            }
            else
            {
                return dps.ToString("#,##0.00");
            }
        }

        public DamageParser(string folderToMonitor)
        {
            monitoredFolder = folderToMonitor;
            if (flag != 0x6c616972657461) throw new InvalidOperationException("mean :(");
        }

        public void Start(TimeSpan updateFrequency)
        {
            if (logStream != null)
            {
                logStream.Dispose();
                logStream = null;
            }

            updateClock = updateFrequency;
            timeUntilSendManualUpdate = TimeSpan.FromTicks(updateClock.Ticks * 2);

            string latestFilePath = Directory.GetFiles(monitoredFolder).Where(f => Regex.IsMatch(f, @"\d+\.csv")).OrderByDescending(f => f).First();
            FileStream fileStream = File.Open(latestFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fileStream.Seek(0, SeekOrigin.Begin);
            logStream = new StreamReader(fileStream);
            hasLogStartTime = false;

            var damageInstances = getLatestDamageInstances(false);
            foreach (var instance in damageInstances) CheckIfSelfInstance(instance);
            if (updateTimer != null)
            {
                updateTimer.Dispose();
                updateTimer = null;
            }

            updateTimer = new Timer(updateTick, null, updateFrequency, updateFrequency);
            ZanversePlayer = new PSO2Player("Zanverse", long.MaxValue, updateClock, InstanceUpdateHistoryDuration, PSO2DamageTrackers.Zanverse);
        }

        public string GenerateSummary(bool endSession)
        {
            resetParserOnNewInstance = endSession;
            return summary_generateSummary();
        }

        /// <summary>
        /// Sets the trackers to combine in TotalDamage for each player. Changing this after parsing started does not recalculate old values.
        /// </summary>
        /// <param name="trackersToCombine">Bitflags for trackers to combine</param>
        public void SetTrackersToSumInTotalDamage(PSO2DamageTrackers trackersToCombine)
        {
            trackersToSum = trackersToCombine;
            foreach (var player in _players) player.Value.SetTrackersToIncludeInTotalDamage(trackersToSum);
        }

        public void Reset()
        {
            lock (updateLock)
            {
                internalReset();
            }
        }

        private void internalReset()
        {
            _players.Clear();
            lastUpdateTime = TimeSpan.Zero;
            lastDamageInstance = null;
            hasLogStartTime = false;
            timeLogStarted = DateTime.MinValue;
            damageInstancesQueued = 0;
            ZanversePlayer = new PSO2Player("Zanverse", long.MaxValue, updateClock, InstanceUpdateHistoryDuration, PSO2DamageTrackers.Zanverse);
        }

        private TimeSpan GetElapsedTime()
        {
            if (!hasLogStartTime) return TimeSpan.Zero;
            if (lastDamageInstance == null) return TimeSpan.Zero;
            return lastDamageInstance.Timestamp - timeLogStarted;
        }

        private void updateTick(object user)
        {
            lock (updateLock)
            {
                var damageInstances = getLatestDamageInstances();
                NewDamageInstanceCount = damageInstances.Count;
                if (NewDamageInstanceCount > 0 && resetParserOnNewInstance)
                {
                    resetParserOnNewInstance = false;
                    internalReset();
                    foreach (var instance in damageInstances) EnsurePlayerExists(instance); //since we clear the _players in internalReset, we must ensure all players exist again.
                }
                foreach (var instance in damageInstances)
                {
                    if (!hasLogStartTime)
                    {
                        hasLogStartTime = true;
                        timeLogStarted = instance.Timestamp;
                        NewSessionStarted?.Invoke(this, EventArgs.Empty);
                    }
                    instance.UpdateLogStartTime(timeLogStarted);
                    lastDamageInstance = instance;
                    if (_players.ContainsKey(instance.SourceId))
                    {
                        var sourcePlayer = _players[instance.SourceId];
                        sourcePlayer.AddDamageInstance(instance);
                    }
                    if (_players.ContainsKey(instance.TargetId))
                    {
                        var targetPlayer = _players[instance.TargetId];
                        targetPlayer.AddDamageInstance(instance);
                    }
                    if (instance.IsZanverseDamage && IsZanverseSplit)
                    {
                        ZanversePlayer.AddZanverseDamageInstance(instance);
                    }
                    damageInstancesQueued++;
                    //should send out an update every time updateClock interval is passed between last update and this.
                    //allows an old log to be pasted at once and still read out dps accurately over time
                    while (lastDamageInstance.RelativeTimestamp - lastUpdateTime > updateClock)
                    {
                        DoUpdateTick();
                    }
                }

                if (timeLastUpdateInvoked != DateTime.MinValue && DateTime.Now - timeLastUpdateInvoked > timeUntilSendManualUpdate && damageInstancesQueued > 0)
                {
                    DoUpdateTick();
                }
            }
        }

        private void DoUpdateTick()
        {
            lastUpdateTime += updateClock;
            damageInstancesQueued = 0;

            foreach (var player in _players) player.Value.RecalculateDPS(lastUpdateTime);
            
            TotalFriendlyDamage = _players.Select(p => p.Value.FilteredDamage.TotalDamage).Sum();
            TotalFriendlyDPS = _players.Select(p => p.Value.FilteredDamage.TotalDPS).Sum();
            HighestDpsPlayer = _players.Select(p => p.Value).OrderByDescending(p => p.FilteredDamage.TotalDamage).FirstOrDefault();

            foreach (var player in _players) player.Value.UpdateRelativeDps(TotalFriendlyDamage);
            UpdateSpecialPlayers();

            UpdateTick?.Invoke(this, new UpdateTickEventArgs(lastUpdateTime));
            timeLastUpdateInvoked = DateTime.Now;
            foreach (var player in _players) player.Value.UpdateTick();
            TickSpecialPlayers();
        }

        //for future special players
        private void UpdateSpecialPlayers()
        {
            ZanversePlayer.RecalculateDPS(lastUpdateTime);
        }

        private void TickSpecialPlayers()
        {
            ZanversePlayer.UpdateTick();
        }

        private bool CheckIfSelfInstance(PSO2DamageInstance damageInstance)
        {
            if (damageInstance.SourceName == "YOU")
            {
                selfPlayerId = damageInstance.SourceId;
                Console.WriteLine($"Found self ID {selfPlayerId}");
                return true;
            }
            return false;
        }

        private List<PSO2DamageInstance> getLatestDamageInstances(bool ensurePlayersExist = true)
        {
            List<PSO2DamageInstance> instances = new List<PSO2DamageInstance>();

            string lastReadBuffer = logStream.ReadToEnd();
            foreach (var currentLine in lastReadBuffer.Split('\n').Select(l => l.Trim('\r')))
            {
                if (string.IsNullOrWhiteSpace(currentLine)) continue;
                string[] parts = currentLine.Split(',');
                if (parts.Length != 13) continue;

                var damageInstance = new PSO2DamageInstance(parts);
                if(ensurePlayersExist) EnsurePlayerExists(damageInstance);
                if (CheckIfSelfInstance(damageInstance)) continue; //don't log this.
                if (damageInstance.InstanceId == -1 || damageInstance.SourceId == 0 || damageInstance.TargetId == 0 || damageInstance.AttackId == 0) continue; //invalid
                if (damageInstance.Damage < 1) continue; //not needed, not logging healing, no overheal check so whats the point
                instances.Add(damageInstance);
            }
            if (instances.Count != 0) Console.WriteLine($"Got {instances.Count} new damage instances");
            return instances;
        }

        private void EnsurePlayerExists(PSO2DamageInstance instance)
        {
            if (PSO2Player.IsAllyId(instance.SourceId))
            {
                if (!Players.ContainsKey(instance.SourceId))
                {
                    PSO2Player player = new PSO2Player(instance.SourceName, instance.SourceId, updateClock, InstanceUpdateHistoryDuration, trackersToSum);
                    _players.Add(player.ID, player);
                    Console.WriteLine($"Creating PSO2Player for source id {instance.SourceId}, name {instance.SourceName}");
                }
            }
            if (PSO2Player.IsAllyId(instance.TargetId))
            {
                if (!Players.ContainsKey(instance.TargetId))
                {
                    PSO2Player player = new PSO2Player(instance.TargetName, instance.TargetId, updateClock, InstanceUpdateHistoryDuration, trackersToSum);
                    _players.Add(player.ID, player);
                    Console.WriteLine($"Creating PSO2Player for target id {instance.TargetId}, name {instance.TargetName}");
                }
            }
        }
    }
}
