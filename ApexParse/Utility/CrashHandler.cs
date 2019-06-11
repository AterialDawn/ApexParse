﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ApexParse.Utility
{
    //Generic utility class.
    static class CrashHandler
    {
        private static bool _initialized = false;
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Application \"{assemblyName}\" unhandled exception")
                .AppendLine($"Timestamp : {DateTime.Now:F}")
                .AppendLine();
            Exception exc = e.ExceptionObject as Exception;
            if (exc == null)
            {
                sb.AppendLine($"Exception is null? IsTerminating = {e.IsTerminating}");
            }
            else
            {
                sb.AppendLine($"IsTerminating = {e.IsTerminating}")
                    .AppendLine($"Exception object dump : \n{exc}");
            }

            string logText = sb.ToString();
            string logName = DateTime.Now.ToString(@"yy\-MM\-dd HH\-mm\-ss tt");
            bool crashLogWritten = WriteCrashLogToFile(logText, logName);
            //spin up a STA thread so that messagebox/clipboard features work properly, since i cant be 100% sure that the thread that calls this eventhandler is STA
            Thread guiThread = new Thread(MessageBoxThread);
            guiThread.SetApartmentState(ApartmentState.STA);
            MessageBoxThreadArgs args = new MessageBoxThreadArgs() { AssemblyName = assemblyName, CrashLogWritten = crashLogWritten, LogName = logName, LogText = logText };
            guiThread.Start(args);
            guiThread.Join();
        }

        private static void MessageBoxThread(object obj)
        {
            var args = obj as MessageBoxThreadArgs;
            if (MessageBox.Show($"Whoops! {args.AssemblyName} has encountered an error and needs to close.\n\n{(args.CrashLogWritten ? $"Crash Log saved to CrashLogs/{args.LogName}.log\n" : "Crash log was unable to be saved.\n")}Copy crash log to clipboard?", $"{args.AssemblyName} has crashed!", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                Clipboard.SetText(args.LogText);
            }
        }

        private static bool WriteCrashLogToFile(string contents, string logName)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo("CrashLogs");
                if (!di.Exists) di.Create();
                string finalDestination = Path.Combine("CrashLogs", logName + ".log");
                File.WriteAllText(finalDestination, contents);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private class MessageBoxThreadArgs
        {
            public string AssemblyName { get; set; }
            public bool CrashLogWritten { get; set; }
            public string LogName { get; set; }
            public string LogText { get; set; }
        }
    }
}
