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

        private Dictionary<long, PlayerDamageSeparationController> _players = new Dictionary<long, PlayerDamageSeparationController>();
        public IEnumerable<PSO2Player> Players { get { return _players.Values.SelectMany(p => p.Players); } } //returns all players that the separation controller generates, 

        public event EventHandler NewSessionStarted;

        public long TotalFriendlyDamage { get; private set; } = 0;
        public double TotalFriendlyDPS { get; private set; } = 0;
        public bool AreNamesAnonimized { get; private set; } = false;
        public long SelfPlayerID { get { return selfPlayerId; } }

        public PSO2Player SelfPlayer { get { return _players.ContainsKey(selfPlayerId) ? _players[selfPlayerId].CombinedPlayer : null; } }
        public PSO2Player HighestDpsPlayer { get; private set; } = null;
        public PSO2Player ZanversePlayer { get; private set; } = null;
        public TimeSpan InstanceUpdateHistoryDuration { get; private set; } = TimeSpan.FromSeconds(5); //5 second history on instance updates
        public TimeSpan AutoEndTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public DateTime LogStartTime { get { return hasLogStartTime ? timeLogStarted : DateTime.MinValue; } }
        public int NewDamageInstanceCount { get; private set; } = 0;
        public bool ParsingActive { get { return hasLogStartTime; } }
        public bool IsZanverseSplit { get { return !trackersToSum.HasFlag(PSO2DamageTrackers.Zanverse); } }
        public bool AutoEndSession { get; set; } = false;

        public event EventHandler<UpdateTickEventArgs> UpdateTick;
        public event EventHandler NameAnonimizationChangedEvent;
        public event EventHandler AutoEndSessionEvent;

        private string monitoredFolder;
        private StreamReader logStream = null;
        private Timer updateTimer = null;
        private DateTime timeLastUpdateInvoked = DateTime.MinValue;
        private TimeSpan timeUntilSendManualUpdate; //after this time elapses, send out another UpdateTick containing the final values. avoids stale data if we got damage instances that dont trigger the UpdateFrequency timer
        private object updateLock = new object();
        private long selfPlayerId = -1;
        private TimeSpan updateClock = TimeSpan.Zero;
        DateTime timeLogStarted = DateTime.MinValue;
        DateTime timeLastLogScanned = DateTime.MinValue;
        TimeSpan lastUpdateTime = TimeSpan.Zero;
        PSO2DamageInstance lastDamageInstance = null;
        bool hasLogStartTime = false;
        bool resetParserOnNewInstance = false;
        int damageInstancesQueued = 0;
        string lastOpenedFile = "";
        PSO2DamageTrackers trackersToSum = PSO2DamageTrackers.All;
        PSO2DamageTrackers trackersToSuppress = PSO2DamageTrackers.None;
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
            Console.WriteLine($"Using folder {folderToMonitor} as log file source");
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

            string latestFilePath = getLatestLogFile();
            initializeLogFile(latestFilePath);
            var damageInstances = getLatestDamageInstances(false);
            foreach (var instance in damageInstances) CheckIfSelfInstance(instance);
            if (updateTimer != null)
            {
                updateTimer.Dispose();
                updateTimer = null;
            }

            updateTimer = new Timer(updateTick, null, updateFrequency, updateFrequency);
        }

        public string GenerateSummary(bool endSession)
        {
            resetParserOnNewInstance = endSession;
            return summary_generateSummary();
        }

        public bool DoesPlayerIdExist(long playerId)
        {
            return _players.ContainsKey(playerId);
        }

        /// <summary>
        /// Sets the trackers to combine in TotalDamage for each player. Changing this after parsing started does not recalculate old values.
        /// </summary>
        /// <param name="trackersToCombine">Bitflags for trackers to combine</param>
        public void SetTrackersToSumInTotalDamage(PSO2DamageTrackers trackersToCombine)
        {
            trackersToSum = trackersToCombine;
        }

        /// <summary>
        /// Sets trackers to suppress. When suppressed, a PSO2Player is not created for the damage type if split, and the damage is not added to the combined player
        /// </summary>
        /// <param name="trackersToHide"></param>
        public void SetTrackersToHide(PSO2DamageTrackers trackersToHide)
        {
            trackersToSuppress = trackersToHide;
        }

        public void Reset()
        {
            lock (updateLock)
            {
                internalReset();
            }
        }

        public void SetNameAnonimization(bool enabled)
        {
            AreNamesAnonimized = enabled;
            NameAnonimizationChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void initializeLogFile(string path)
        {
            Console.WriteLine($"Loading file {path}");
            FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fileStream.Seek(0, SeekOrigin.Begin);
            if (logStream != null)
            {
                logStream.Dispose();
                logStream = null;
            }
            logStream = new StreamReader(fileStream);
            lastOpenedFile = path;
        }

        private string getLatestLogFile()
        {
            return Directory.GetFiles(monitoredFolder).Where(f => Regex.IsMatch(f, @"\d+\.csv")).OrderByDescending(f => long.Parse(Path.GetFileNameWithoutExtension(f))).First();
        }

        private void internalReset()
        {
            _players.Clear();
            lastUpdateTime = TimeSpan.Zero;
            lastDamageInstance = null;
            hasLogStartTime = false;
            timeLogStarted = DateTime.MinValue;
            damageInstancesQueued = 0;
            timeLastUpdateInvoked = DateTime.MinValue;
            ZanversePlayer = null;
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
                if ((DateTime.Now - timeLastLogScanned).TotalSeconds > 10)
                {
                    timeLastLogScanned = DateTime.Now;
                    string latestLogFile = getLatestLogFile();
                    if (latestLogFile != lastOpenedFile)
                    {
                        initializeLogFile(latestLogFile);
                    }
                }
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
                    if (instance.IsZanverseDamage && IsZanverseSplit && !trackersToSuppress.HasFlag(PSO2DamageTrackers.Zanverse))
                    {
                        if (ZanversePlayer == null)
                        {
                            ZanversePlayer = new PSO2Player("Zanverse", long.MaxValue, updateClock, InstanceUpdateHistoryDuration, PSO2DamageTrackers.Zanverse, this);
                            ZanversePlayer.SetSpecialPlayer(true, false);
                        }
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
                    NewDamageInstanceCount = damageInstancesQueued; //bugfix, NewDamageInstanceCount being set to 0 by above call is incorrect, since there actually are new instances being processed
                    DoUpdateTick();
                }

                if (AutoEndSession && timeLastUpdateInvoked != DateTime.MinValue && DateTime.Now - timeLastUpdateInvoked > AutoEndTimeout && !resetParserOnNewInstance)
                {
                    Console.WriteLine("Notifying to end session");
                    AutoEndSessionEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void DoUpdateTick()
        {
            lastUpdateTime += updateClock;

            foreach (var player in Players) player.UpdateTick();
            TickSpecialPlayers();

            foreach (var player in Players) player.RecalculateDPS(lastUpdateTime);
            
            TotalFriendlyDamage = Players.Select(p => p.FilteredDamage.TotalDamage).Sum();
            TotalFriendlyDPS = Players.Select(p => p.FilteredDamage.TotalDPS).Sum();
            HighestDpsPlayer = Players.OrderByDescending(p => p.FilteredDamage.TotalDamage).FirstOrDefault();

            foreach (var player in Players) player.UpdateRelativeDps(TotalFriendlyDamage);
            UpdateSpecialPlayers();

            UpdateTick?.Invoke(this, new UpdateTickEventArgs(lastUpdateTime));
            timeLastUpdateInvoked = DateTime.Now;
            damageInstancesQueued = 0;
        }

        //for future special players
        private void UpdateSpecialPlayers()
        {
            ZanversePlayer?.RecalculateDPS(lastUpdateTime);
        }

        private void TickSpecialPlayers()
        {
            ZanversePlayer?.UpdateTick();
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
                if (CheckIfSelfInstance(damageInstance)) continue; //don't log this.
                if (ensurePlayersExist) EnsurePlayerExists(damageInstance);
                if (damageInstance.InstanceId == -1 || damageInstance.SourceId == 0 || damageInstance.TargetId == 0 || damageInstance.AttackId == 0) continue; //invalid
                if (damageInstance.Damage < 1) continue; //not needed, not logging healing, no overheal check so whats the point
                instances.Add(damageInstance);
            }
            return instances;
        }

        private void EnsurePlayerExists(PSO2DamageInstance instance)
        {
            if (PSO2Player.IsAllyId(instance.SourceId))
            {
                if (!_players.ContainsKey(instance.SourceId))
                {
                    var controller = new PlayerDamageSeparationController(instance.SourceName, instance.SourceId, updateClock, InstanceUpdateHistoryDuration, this, trackersToSum);
                    _players.Add(instance.SourceId, controller);
                    Console.WriteLine($"Creating PSO2Player for source id {instance.SourceId}, name {instance.SourceName}");
                }
            }
            if (PSO2Player.IsAllyId(instance.TargetId))
            {
                if (!_players.ContainsKey(instance.TargetId))
                {
                    var controller = new PlayerDamageSeparationController(instance.TargetName, instance.TargetId, updateClock, InstanceUpdateHistoryDuration, this, trackersToSum);
                    _players.Add(instance.TargetId, controller);
                    Console.WriteLine($"Creating PSO2Player for target id {instance.TargetId}, name {instance.TargetName}");
                }
            }
        }

        class PlayerDamageSeparationController
        {
            public List<PSO2Player> Players { get; private set; } = new List<PSO2Player>();
            public PSO2Player CombinedPlayer { get; private set; }
            private Dictionary<PSO2DamageTrackers, PSO2Player> playerTrackerDict = new Dictionary<PSO2DamageTrackers, PSO2Player>();

            PSO2DamageTrackers trackerFlags;
            DamageParser parser;

            string name;
            long id;
            TimeSpan updateInterval;

            public PlayerDamageSeparationController(string playerName, long playerId, TimeSpan updateClock, TimeSpan instanceHistory, DamageParser owner, PSO2DamageTrackers activeTrackers)
            {
                trackerFlags = activeTrackers;
                name = playerName;
                id = playerId;
                updateInterval = updateClock;

                parser = owner;
                
                CombinedPlayer = new PSO2Player(playerName, playerId, updateClock, instanceHistory, activeTrackers, owner);
                Players.Add(CombinedPlayer);
            }

            public void AddDamageInstance(PSO2DamageInstance instance)
            {
                //this sends the damage instance to the correct split player, or the combined player if this instance isnt split
                //dispatchInstance returns true when damagetype matches split player, once true, wereAnyDispatched will be set to true.
                //we dont deal with ZV separation, since all players are accumulated for that, it is handled in DamageParser class itself
                bool wereAnyDispatched = dispatchInstance(instance.IsAISDamage, PSO2DamageTrackers.AIS, instance, "AIS");
                wereAnyDispatched |= dispatchInstance(instance.IsDarkBlastDamage, PSO2DamageTrackers.DarkBlast, instance, "DB");
                wereAnyDispatched |= dispatchInstance(instance.IsHeroFinishDamage, PSO2DamageTrackers.HTF, instance, "HTF");
                wereAnyDispatched |= dispatchInstance(instance.IsLaconiumDamage, PSO2DamageTrackers.LSW, instance, "LSW");
                wereAnyDispatched |= dispatchInstance(instance.IsPhotonDamage, PSO2DamageTrackers.PWP, instance, "PWP");
                wereAnyDispatched |= dispatchInstance(instance.IsRideroidDamage, PSO2DamageTrackers.Ride, instance, "Ride");
                if (!wereAnyDispatched)
                {
                    CombinedPlayer.AddDamageInstance(instance);
                }
                
            }

            void buildSplitPlayer(PSO2DamageTrackers tracker, string nameSuffix)
            {
                if (IsSplit(tracker))
                {
                    if (parser.trackersToSuppress.HasFlag(tracker)) return; //do nothing if tracker is suppressed
                    var player = new PSO2Player($"{name} | {nameSuffix}", id, updateInterval, parser.InstanceUpdateHistoryDuration, tracker, parser);
                    player.SetSpecialPlayer(true, true);
                    playerTrackerDict.Add(tracker, player);
                    Players.Add(player);
                }
            }

            bool dispatchInstance(bool isCorrectType, PSO2DamageTrackers tracker, PSO2DamageInstance instance, string nameSuffix)
            {
                if (!isCorrectType) return false;
                if (parser.trackersToSuppress.HasFlag(tracker)) return true; //swallow this damage instance if it's suppressed.
                else if (IsSplit(tracker))
                {
                    if (!playerTrackerDict.ContainsKey(tracker))
                    {
                        buildSplitPlayer(tracker, nameSuffix);
                    }
                    playerTrackerDict[tracker].AddDamageInstance(instance);
                    return true;
                }
                return false;
            }

            bool IsSplit(PSO2DamageTrackers tracker)
            {
                return !trackerFlags.HasFlag(tracker);
            }
        }
    }
}
