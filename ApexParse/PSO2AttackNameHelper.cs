using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ApexParse
{
    static class PSO2AttackNameHelper
    {
        static Dictionary<long, string> _attackDict = new Dictionary<long, string>();
        static List<long> _ignoredAttacksForJA = new List<long>();
        static List<long> _heroFinishIds = new List<long>();
        static List<long> _photonIds = new List<long>();
        static List<long> _aisIds = new List<long>();
        static List<long> _rideIds = new List<long>();
        static List<long> _darkBlastIds = new List<long>();
        static List<long> _laconiumIds = new List<long>();
        static List<long> _elemIds = new List<long>();

        const string IgnoredSkillsCsv = "ignoredskills.csv";
        const string SkillsCsv = "skills.csv";
        const string SeparateXml = "separateAttacks.xml";

        internal static void Initialize(bool forceDownload = false)
        {
            bool skillsExist = File.Exists("skills.csv");
            if (forceDownload || !skillsExist)
            {
                DownloadSkillsCsv();
            }
            ParseSkillsCsv();
        }

        internal static bool IsHeroFinishAttack(long id) => _heroFinishIds.Contains(id);
        internal static bool IsPhotonAttack(long id) => _photonIds.Contains(id);
        internal static bool IsAisAttack(long id) => _aisIds.Contains(id);
        internal static bool IsRideroidAttack(long id) => _rideIds.Contains(id);
        internal static bool IsDarkBlastAttack(long id) => _darkBlastIds.Contains(id);
        internal static bool IsLaconiumAttack(long id) => _laconiumIds.Contains(id);
        internal static bool IsElementalDamage(long id) => _elemIds.Contains(id);
        internal static bool IsIgnoredAttackForJA(long id) => _ignoredAttacksForJA.Contains(id);

        internal static string GetAttackName(long id)
        {
            if (_attackDict.ContainsKey(id)) return _attackDict[id];
            return "Unknown";
        }

        private static void DownloadSkillsCsv()
        {
            WebClient wc = new WebClient();
            try
            {
                string tempSkills = SkillsCsv + "_temp";

                if (File.Exists(tempSkills)) File.Delete(tempSkills);
                wc.DownloadFile("https://raw.githubusercontent.com/VariantXYZ/PSO2ACT/master/PSO2ACT/skills.csv", tempSkills);
                if (File.Exists(SkillsCsv))
                {
                    Console.WriteLine("Deleting skills.csv");
                    File.Delete(SkillsCsv);
                }
                File.Move(tempSkills, SkillsCsv);
                Console.WriteLine("skills.csv updated!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error updating skills.csv\n{e.ToString()}");
            }

            try
            {
                string tempIgnored = IgnoredSkillsCsv + "_temp";

                if (File.Exists(tempIgnored)) File.Delete(tempIgnored);
                wc.DownloadFile("https://raw.githubusercontent.com/mysterious64/OverParse/master/OverParse/Updates/ignoreskills.csv", tempIgnored);
                if (File.Exists(IgnoredSkillsCsv))
                {
                    Console.WriteLine("Deleting ignoredskills.csv");
                    File.Delete(IgnoredSkillsCsv);
                }
                File.Move(tempIgnored, IgnoredSkillsCsv);
                Console.WriteLine("ignoredskills.csv updated!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error updating ignoredskills.csv\n{e.ToString()}");
            }

            try
            {
                string tempSeparate = SeparateXml + "_temp";

                if (File.Exists(tempSeparate)) File.Delete(tempSeparate);
                wc.DownloadFile("https://gist.githubusercontent.com/AterialDawn/9c5212b736b4ca2d0b75f5857e1c1547/raw/separateAttacks.xml", tempSeparate);
                if (File.Exists(SeparateXml))
                {
                    Console.WriteLine("Deleting separateAttacks.xml");
                    File.Delete(SeparateXml);
                }
                File.Move(tempSeparate, SeparateXml);
                Console.WriteLine("separateAttacks.xml updated!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error updating separateAttacks.xml\n{e.ToString()}");
            }
        }

        private static void ParseSkillsCsv()
        {
            if (File.Exists(SkillsCsv))
            {
                try
                {
                    string[] skillsLines = File.ReadAllLines(SkillsCsv);
                    if (skillsLines.Length == 0) return;

                    foreach (var line in skillsLines)
                    {
                        string[] currentParts = line.Split(',');
                        if (currentParts.Length != 4) continue;
                        if (currentParts[0] == "Type") continue; //header line
                        long attackId = 0;
                        if (!long.TryParse(currentParts[1], out attackId)) continue;

                        _attackDict.Add(attackId, currentParts[0]);
                    }

                    Console.WriteLine($"There are {_attackDict.Count} registered attack names");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected error parsing skills.csv\n{e.ToString()}");
                }
            }
            else
            {
                Console.WriteLine($"skills.csv doesn't exist!");
            }

            if (File.Exists(IgnoredSkillsCsv))
            {
                try
                {
                    foreach (var line in File.ReadAllLines(IgnoredSkillsCsv))
                    {
                        long val;
                        if (!long.TryParse(line, out val)) continue;
                        _ignoredAttacksForJA.Add(val);
                    }

                    Console.WriteLine($"There are {_ignoredAttacksForJA.Count} ignored JA attacks");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected error parsing ignoredskills.csv\n{e.ToString()}");
                }
            }
            else
            {
                Console.WriteLine("ignoredskills.csv does't exist!");
            }

            //Parse separateAttacks.xml
            try
            {
                XElement doc = XElement.Load("separateAttacks.xml");
                foreach (var attackElement in doc.Elements())
                {
                    List<long> listToLoad = null;
                    switch (attackElement.Name.LocalName)
                    {
                        case "heroFinish":
                            listToLoad = _heroFinishIds; break;
                        case "photon":
                            listToLoad = _photonIds; break;
                        case "ais":
                            listToLoad = _aisIds; break;
                        case "ride":
                            listToLoad = _rideIds; break;
                        case "darkBlast":
                            listToLoad = _darkBlastIds; break;
                        case "laconium":
                            listToLoad = _laconiumIds; break;
                        case "element":
                            listToLoad = _elemIds; break;
                    }
                    if (listToLoad != null) loadElementIdsToList(attackElement, listToLoad);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to load separateAtacks.xml!\n{e}");
            }
        }

        static void loadElementIdsToList(XElement element, List<long> list)
        {
            if (list == null)
            {
                Console.WriteLine($"Unknown XElement list mapping for element name {element.Name.LocalName}");
                return;
            }

            foreach (var attackId in element.Descendants())
            {
                long id = 0;
                if (!long.TryParse(attackId.Value, out id))
                {
                    Console.WriteLine($"{element.Name.LocalName} xml parse error! Expected number, got {attackId.Value}");
                    continue;
                }

                list.Add(id);
                Console.WriteLine($"Registering split attack id {id}");
            }
        }
    }
}
