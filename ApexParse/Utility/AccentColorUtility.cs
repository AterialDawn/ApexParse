using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Newtonsoft.Json;
using MahApps.Metro;
using XamlColorSchemeGenerator;
using ColorScheme = XamlColorSchemeGenerator.ColorScheme;
using ApexParse.Properties;
using System.Reflection;

namespace ApexParse.Utility
{
    static class AccentColorUtility
    {
        public static void ReloadActiveColor()
        {
            if (Settings.Default.CustomAccentColor != System.Drawing.Color.Transparent)
            {
                var col = Settings.Default.CustomAccentColor;
                var mediaColor = Color.FromArgb(col.A, col.R, col.G, col.B);
                CreateTheme("Dark", mediaColor, changeImmediately: true);
            }
            else
            {
                ThemeManager.ChangeAppStyle(App.Current, ThemeManager.GetAccent("Purple"), ThemeManager.GetAppTheme("BaseDark"));
            }
        }

        public static ResourceDictionary CreateTheme(string baseColorScheme, Color accentBaseColor, string name = null, bool changeImmediately = false)
        {
            name = name ?? $"RuntimeTheme_{baseColorScheme}_{accentBaseColor.ToString().Replace("#", string.Empty)}";

            var generatorParameters = GetGeneratorParameters();
            var themeTemplateContent = GetThemeTemplateContent();

            var variant = generatorParameters.BaseColorSchemes.First(x => x.Name == baseColorScheme);
            var colorScheme = new ColorScheme();
            colorScheme.Name = accentBaseColor.ToString().Replace("#", string.Empty);
            var values = colorScheme.Values;
            values.Add("AccentBaseColor", accentBaseColor.ToString());
            values.Add("AccentColor", Color.FromArgb(204, accentBaseColor.R, accentBaseColor.G, accentBaseColor.B).ToString());
            values.Add("AccentColor2", Color.FromArgb(153, accentBaseColor.R, accentBaseColor.G, accentBaseColor.B).ToString());
            values.Add("AccentColor3", Color.FromArgb(102, accentBaseColor.R, accentBaseColor.G, accentBaseColor.B).ToString());
            values.Add("AccentColor4", Color.FromArgb(51, accentBaseColor.R, accentBaseColor.G, accentBaseColor.B).ToString());

            values.Add("HighlightColor", accentBaseColor.ToString());
            values.Add("IdealForegroundColor", IdealTextColor(accentBaseColor).ToString());

            var xamlContent = new ColorSchemeGenerator().GenerateColorSchemeFileContent(generatorParameters, variant, colorScheme, themeTemplateContent, name, name);

            var resourceDictionary = (ResourceDictionary)XamlReader.Parse(xamlContent);
            
            ThemeManager.AddAppTheme(name, resourceDictionary);

            // Apply theme
            if (changeImmediately)
            {
                ThemeManager.ChangeAppTheme(Application.Current, name);
            }

            return resourceDictionary;
        }


        private static Color IdealTextColor(Color color)
        {
            const int nThreshold = 105;
            var bgDelta = Convert.ToInt32((color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114));
            var foreColor = 255 - bgDelta < nThreshold
                ? Colors.Black
                : Colors.White;
            return foreColor;
        }

        private static string GetThemeTemplateContent()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ApexParse.ThemeTemplate.xaml"))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static GeneratorParameters GetGeneratorParameters()
        {
            return JsonConvert.DeserializeObject<GeneratorParameters>(GetGeneratorParametersJson());
        }

        private static string GetGeneratorParametersJson()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ApexParse.GeneratorParameters.json"))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
