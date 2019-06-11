using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse
{
    static class PSO2AttackNameHelper
    {
        static Dictionary<long, string> _attackDict = new Dictionary<long, string>();
        static List<long> _ignoredAttacksForJA = new List<long>();

        const string IgnoredSkillsCsv = "ignoredskills.csv";
        const string SkillsCsv = "skills.csv";

        internal static void Update(bool forceDownload = false)
        {
            _attackDict.Clear();
            _ignoredAttacksForJA.Clear();
            bool skillsExist = File.Exists("skills.csv");
            if (forceDownload || !skillsExist)
            {
                DownloadSkillsCsv();
            }
            ParseSkillsCsv();
        }

        internal static string GetAttackName(long id)
        {
            if (_attackDict.ContainsKey(id)) return _attackDict[id];
            return "Unknown";
        }

        internal static bool IsIgnoredAttackForJA(long id)
        {
            return _ignoredAttacksForJA.Contains(id);
        }

        private static void DownloadSkillsCsv()
        {
            WebClient wc = new WebClient();
            try
            {
                if (File.Exists(SkillsCsv))
                {
                    Console.WriteLine("Deleting skills.csv");
                    File.Delete(SkillsCsv);
                }
                
                wc.DownloadFile("https://raw.githubusercontent.com/VariantXYZ/PSO2ACT/master/PSO2ACT/skills.csv", SkillsCsv);
                Console.WriteLine("skills.csv updated!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error updating skills.csv\n{e.ToString()}");
            }

            try
            {
                if (File.Exists(IgnoredSkillsCsv))
                {
                    Console.WriteLine("Deleting ignoredskills.csv");
                    File.Delete(IgnoredSkillsCsv);
                }

                wc.DownloadFile("https://raw.githubusercontent.com/mysterious64/OverParse/master/OverParse/Updates/ignoreskills.csv", IgnoredSkillsCsv);
                Console.WriteLine("ignoredskills.csv updated!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error updating ignoredskills.csv\n{e.ToString()}");
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
        }
    }
}
