using ApexParse.Properties;
using ApexParse.Utility;
using ApexParse.ViewModel;
using Aterial.Utility;
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace ApexParse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static string VersionString { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            VersionString = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CrashHandler.Initialize();
            PSO2AttackNameHelper.Initialize(true); //force download every time, small file anyways.

            UpgradeSettings();
            ParseArgs(e.Args);

            AccentColorUtility.ReloadActiveColor();

            RenderOptions.ProcessRenderMode = Settings.Default.SoftwareRenderingEnabled ? System.Windows.Interop.RenderMode.SoftwareOnly : System.Windows.Interop.RenderMode.Default; //set this before creating any windows

            var window = new MainWindow();
            App.Current.MainWindow = window;
            window.ViewModel = new MainWindowViewModel();
            window.Show();
        }

        private void ParseArgs(string[] args)
        {
            if (args == null) return;
            CommandLineParser parser = new CommandLineParser();
            parser.AddOption("tickrate", "Parser Tick Rate Override", true);
            parser.AddOption("linewidth", "Width of lines in Graph Display", true);
            parser.Parse(args);

            var tickRateOption = parser.ActiveOptions.Where(t => t.Item1.ToLower() == "tickrate").FirstOrDefault();
            if (tickRateOption != null)
            {
                double newTickRate = 0;
                if (!double.TryParse(tickRateOption.Item2, out newTickRate)) { newTickRate = -1; }
                if (newTickRate < 0)
                {
                    MessageBox.Show($"Invalid Parameter passed to -TickRate option! Must be a positive number.", "Error!", MessageBoxButton.OK);
                    throw new InvalidOperationException($"Invalid Argument passed to -TickRate! Was {tickRateOption.Item2 ?? "NULL"}");
                }
                Settings.Default.ParserTickRate = newTickRate;
            }

            var lineWidthOption = parser.ActiveOptions.Where(t => t.Item1.ToLower() == "linewidth").FirstOrDefault();
            if (lineWidthOption != null)
            {
                double newWidth = 0;
                if (!double.TryParse(lineWidthOption.Item2, out newWidth)) { newWidth = -1; }
                if (newWidth < 0)
                {
                    MessageBox.Show($"Invalid Parameter passed to -LineWidth option! Must be a positive number.", "Error!", MessageBoxButton.OK);
                    throw new InvalidOperationException($"Invalid Argument passed to -LineWidth! Was {tickRateOption.Item2 ?? "NULL"}");
                }
                Settings.Default.LineStrokeWidth = newWidth;
            }
        }

        private void UpgradeSettings()
        {
            if (!Settings.Default.UpgradeComplete)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeComplete = true;
                Settings.Default.Save();
            }
        }
    }
}
