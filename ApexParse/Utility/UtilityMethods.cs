using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse.Utility
{
    class UtilityMethods
    {
        static internal void EnsureFolderExists(string folder)
        {
            DirectoryInfo info = new DirectoryInfo(folder);
            if (!info.Exists) info.Create();
        }

        static internal string ReplaceInvalidCharactersInPath(string path)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                path = path.Replace(c.ToString(), "_");
            }

            return path;
        }
        
        static internal void OpenWithDefaultProgram(string path)
        {
            Process.Start(path);
        }
    }
}
