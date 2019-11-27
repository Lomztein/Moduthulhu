using Lomztein.Moduthulhu.Core.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot
{
    /// <summary>
    /// This class is an experiment that didn't really go anywhere. I've left it in for possible future use, though lets be honest that's propably not gonna happen. The current code design simply does not really allow for it to be effectively implemented.
    /// </summary>
    public static class Localization // Should be fairly straight forward to change this to a non-static class, but I decided to make it static for convenience. Alternatively consider having one of these per GuildHandler as to maintain consisitancy in design.
    {
        private static string LocalizationsDirectory => BotCore.DataDirectory + "/Localizations";
        private static Dictionary<CultureInfo, Dictionary<string, Dictionary<string, string>>> _translationTable
            = new Dictionary<CultureInfo, Dictionary<string, Dictionary<string, string>>> (); // A triple nested dictionary? My ancestors must be proud.
        public static CultureInfo DefaultCulture { get; private set; }
         
        private static string FormatName(CultureInfo culture, string identifier) => $"{culture.Name}_{identifier}";
        private static (CultureInfo culture, string identifier) DeformatName(string name)
        {
            string[] halves = name.Split("_");
            return (new CultureInfo(halves[0]), halves[1]);
        }

        public static void Init (CultureInfo defaultCulture)
        {
            DefaultCulture = defaultCulture;
            Log.Write(Log.Type.BOT, $"Initialized localization manager with default culture '{DefaultCulture.EnglishName}'.");
            Directory.CreateDirectory(LocalizationsDirectory);

            CacheFullTanslationTable();
        }

        private static void CacheFullTanslationTable ()
        {
            string[] files = Directory.GetFiles(LocalizationsDirectory);
            foreach (string file in files)
            {
                var (culture, identifier) = DeformatName(Path.GetFileNameWithoutExtension (file));
                Dictionary<string, string> dict = JSONSerialization.DeserializeFile<Dictionary<string, string>>(file);

                if (!_translationTable.ContainsKey(culture))
                {
                    _translationTable.Add(culture, new Dictionary<string, Dictionary<string, string>>());
                }

                if (!_translationTable[culture].ContainsKey(identifier))
                {
                    _translationTable[culture].Add(identifier, null);
                }

                _translationTable[culture][identifier] = dict;
            }
        }

        public static string Get(CultureInfo culture, string identifier, string key) => GetF(culture, identifier, key);

        public static string GetF (CultureInfo culture, string identifier, string key, params object[] objects)
        {
            string missing = culture.Equals (DefaultCulture) ? key : GetF(DefaultCulture, identifier, key, objects);
            bool changed = TrySetMissingEntry(culture, identifier, key, missing); // Set missing entry to either default cultures value, or the identifier if default cultures value is the one that's missing.

            string value = _translationTable[culture][identifier][key];
            for (int i = 0; i < objects.Length; i++)
            {
                value = value.Replace($"{{{i}}}", objects[i].ToString (), StringComparison.Ordinal);
            }

            if (changed)
            {
                StoreTranslationTable(culture, identifier);
            }

            return value;
        }

        private static bool TrySetMissingEntry(CultureInfo culture, string identifier, string key, string value)
        {
            bool changed = false;

            if (!_translationTable.ContainsKey(culture))
            {
                _translationTable.Add(culture, new Dictionary<string, Dictionary<string, string>>());
                changed = true;
            }

            if (!_translationTable[culture].ContainsKey(identifier))
            {
                _translationTable[culture].Add(identifier, new Dictionary<string, string>());
                changed = true;
            }

            if (!_translationTable[culture][identifier].ContainsKey(key))
            {
                _translationTable[culture][identifier].Add(key, value);
                changed = true;
            }

            return changed;
        }

        private static void StoreTranslationTable (CultureInfo culture, string identifier)
        {
            JSONSerialization.SerializeObject(_translationTable[culture][identifier], LocalizationsDirectory + "/" + FormatName(culture, identifier));
        }
    }
}
