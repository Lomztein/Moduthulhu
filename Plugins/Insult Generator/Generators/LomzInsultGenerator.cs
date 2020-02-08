using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.Moduthulhu.Plugins.InsultGenerators.Lomz;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lomztein.Moduthulhu.Plugins.InsultGenerators
{
    public class LomzInsultGenerator : IInsultGenerator
    {
        private static char[] _vowels = new char[] { 'a', 'e', 'i', 'o', 'u', 'y' };

        private const string SOURCE_FILE_NAME = "InsultData.json";
        private InsultDataSource _source;

        private Random random = new Random();
        private Dictionary<string, Func<string, string>> _variables;

        public string Insult(string target)
        {
            CacheSource();
            if (_source == null)
            {
                return $"{target} uses a bot whose host forgot to create the Core/Resources/InsultData.json file from where it reads insult data.";
            }
            else
            {
                return Insult(target, _source);
            }
        }

        public LomzInsultGenerator (GuildHandler parentHandler)
        {
            InitVariables(parentHandler);
        }

        private void InitVariables (GuildHandler dataSource)
        {
            _variables = new Dictionary<string, Func<string, string>>
            {
                { "Target", new Func<string, string> (target => target) },
                { "RandomAdmin", new Func<string, string> (target => SelectRandom (dataSource.GetGuild().Users.Where (x => x.GuildPermissions.Administrator)).GetShownName ()) }
            };
        }

        private T SelectRandom<T> (IEnumerable<T> enumerable)
        {
            return enumerable.ToList()[random.Next(enumerable.Count())];
        }

        private string Insult (string target, InsultDataSource source)
        {
            string format = source.Formats[random.Next (source.Formats.Length)];
            string result = RecursiveHandleReplacements(format, target, source);
            result = RemoveExcessSpaces(result);
            return HandleAns(result);
        }

        private string RemoveExcessSpaces (string input)
        {
            Regex regex = new Regex(@" {2,}");
            return regex.Replace(input, " ");
        }

        private string RecursiveHandleReplacements (string input, string target, InsultDataSource source)
        {
            Regex regex = new Regex(@"{(.*?)}");
            var result = regex.Replace (input, x => HandleReplacement(x.Value, target, source));
            if (input == result)
            {
                return result;
            }
            else
            {
                return RecursiveHandleReplacements(result, target, source);
            }
        }

        private string HandleReplacement(string input, string target, InsultDataSource source)
        {

            // Handle optionals. Replacements ending with ?xxx where xxx referes to chance of appearing in integers between 0 and 100
            input = HandleOptional(input);

            // Handle multiples. Replacements ending with multiple dots indicating a random amount of the same replacement that may potentially repeat.
            string multi = HandleMultiples(input);
            if (multi != input)
            {
                return multi; // Return early, since the input has now been split in multiple and must handled recursively.
            }

            // Handle choosable. Replacements with multiple options split with /.
            input = HandleChoosables(input);

            return GetFromSource(input, target, source);
        }

        private string HandleOptional(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            int indexOfOptional = input.IndexOf('?');
            if (indexOfOptional != -1)
            {
                string contents = input.Substring(1, input.Length - 2);
                string toParse = contents.Substring(indexOfOptional);
                int chance = int.Parse(toParse);
                if (random.Next(101) < chance)
                {
                    input = input.Substring(0, indexOfOptional) + "}";
                }
                else
                {
                    input = string.Empty;
                }
            }

            return input;
        }

        private string HandleMultiples (string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            int start = Math.Max (input.IndexOf('.'), 0);
            int end = input.LastIndexOf('.') + 1;
            int count = random.Next ((end - start) + 1);

            if (start != 0)
            {
                input = input.Substring(0, start) + "}";
            }

            for (int i = 0; i < count; i++)
            {
                input += ", " + input;
            }

            return input;
        }

        private string HandleChoosables (string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            string contents = input.Substring(1, input.Length - 2);
            Random random = new Random();
            string[] options = contents.Split('/');
            return $"{{{options[random.Next(options.Length)]}}}";
        }

        private string GetFromSource (string input, string target, InsultDataSource source)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            string contents = input.Substring(1, input.Length - 2);

            if (contents.StartsWith('$')) // Content refers to a variable
            {
                return source.GetVariable(contents.Substring(1), target);
            }
            return source.Get (contents, target);
        }

        private string HandleAns (string input)
        {
            Regex regex = new Regex(@"a\/an");
            return regex.Replace(input, x => input.Length > x.Index + x.Length + 1 ? (_vowels.Contains(input[x.Index + x.Length + 1]) ? "an" : "a") : "a");
        }

        private void CacheSource ()
        {
            if (_source == null)
            {
                _source = LoadSource(Path.Combine (BotCore.ResourcesDirectory, SOURCE_FILE_NAME));
            }
        }

        private InsultDataSource LoadSource (string path)
        {
            JObject obj = JSONSerialization.LoadAsJObject(path);
            string[] formats = null;
            Dictionary<string, string[]> categories = new Dictionary<string, string[]>();

            foreach (JProperty property in obj.Properties())
            {
                if (property.Name == "Formats")
                {
                    formats = property.First.ToObject<string[]>();
                }
                else
                {
                    categories.Add(property.Name, property.First.ToObject<string[]>());
                }
            }

            return new InsultDataSource(formats, categories, _variables);
        }
    }
}
