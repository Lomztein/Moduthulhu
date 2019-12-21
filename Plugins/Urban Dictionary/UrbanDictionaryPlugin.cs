using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    [Descriptor ("Lomztein", "Urban Dictionary", "Provides a single command for querying for definitions of words from the single most correct and factual source, Urban Dictionary.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/blob/master/Plugins/Urban%20Dictionary/UrbanDictionaryPlugin.cs")]
    public class UrbanDictionaryPlugin : PluginBase
    {
        private readonly ICommand cmd = new UrbanDefineCommand();

        public override void Initialize()
        {
            SendMessage("Lomztein-Command Root", "AddCommand", cmd);
        }

        public override void Shutdown()
        {
            SendMessage("Lomztein-Command Root", "RemoveCommand", cmd);
        }
    }

    public class UrbanDefinition
    {
        public const string ApiUrl = "http://api.urbandictionary.com/v0/define?term={word}";
        public const string SearchUrl = "https://www.urbandictionary.com/define.php?term={word}";

        public readonly string Word;
        public readonly string Definition;
        public readonly string Example;
        public readonly string Author;
        public readonly string Permalink;
        public readonly bool Success;

        public string[] Tags { get; }
        public string[] Sounds { get; }

        readonly int ThumbsUp;
        readonly int ThumbsDown;

        public UrbanDefinition(JObject jObject, string word)
        {
            Success = (jObject["list"] as JArray).Count > 0;

            if (Success)
            {
                JObject first = (jObject["list"] as JArray)[0] as JObject;

                Word = first["word"].ToObject<string>();
                Sounds = first["sound_urls"].ToObject<string[]>();
                Definition = first["definition"].ToObject<string>();
                Example = first["example"].ToObject<string>();
                Author = first["author"].ToObject<string>();
                Permalink = first["permalink"].ToObject<string>();

                ThumbsUp = first["thumbs_up"].ToObject<int>();
                ThumbsDown = first["thumbs_down"].ToObject<int>();
            }
            else
            {
                Word = word;
            }
        }

        public Embed ToEmbed()
        {
            Regex newLineRegex = new Regex(@"\n");

            if (Success)
            {
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle($"Top definition of {Word}")
                    .WithUrl(Permalink)
                    .WithColor(new Color(0, 0, 128))
                    .WithDescription(EscapeEmphasis (EmbedNestedDefinitions(Definition)))
                    .WithFooter($"Defined by {Author}. Souce: www.urbandictionary.com");
                if (Example.Length > 0)
                {
                    builder.AddField("Example", "> " + EscapeEmphasis (string.Join("\n> ", EmbedNestedDefinitions(Example).Split('\n'))));
                }

                builder.AddField("Votes", ThumbsUp.ToString() + "↑ / " + ThumbsDown.ToString() + "↓");

                if (Sounds.Length > 0)
                {
                    int index = 1;
                    IEnumerable<string> sounds = Sounds.Select(x => $"[Sound {index++}]({x})");
                    builder.AddField("Sound", string.Join("\n", sounds.ToList().GetRange(0, Math.Min(Sounds.Length, GetAmountBeforeSize(sounds, 1024))).ToArray()));
                }

                return builder.Build();
            }
            else
            {
                return new EmbedBuilder().WithTitle($"No definitons for {Word} found.").WithColor(new Color(0, 0, 128)).Build();
            }
        }

        private int GetAmountBeforeSize(IEnumerable<string> strings, int maxSize)
        {
            int size = 0;
            int index = 0;
            foreach (string str in strings)
            {
                int newSize = size + str.Length;
                if (newSize > maxSize)
                {
                    return index - 1;
                }
                size = newSize;
                index++;
            }
            return strings.Count();
        }

        private string EmbedNestedDefinitions(string input)
        {
            Regex squareBracketRegex = new Regex(@"\[(.*?)\]");
            string result = squareBracketRegex.Replace(input, x => $"{x.Value}({GetSearchUrl(x.Value.Substring(1, x.Value.Length - 2))})");
            return result;
        }

        private string EscapeEmphasis (string input)
        {
            Regex toEscapeRegex = new Regex(@"_|\*");
            return toEscapeRegex.Replace(input, x => $"\\{x.Value}");
        }

        private string GetSearchUrl(string word) => SearchUrl.Replace("{word}", word).Replace (" ", "%20");

        public static async Task<UrbanDefinition> Get(string word)
        {
            JObject json = await HTTP.GetJSON(new Uri(ApiUrl.Replace("{word}", word))).ConfigureAwait (false);
            return new UrbanDefinition(json, word);
        }

    }

    public class UrbanDefineCommand : Command
    {
        public UrbanDefineCommand()
        {
            Name = "define";
            Description = "Understand linguistics.";
            Category = StandardCategories.Fun;
            Aliases = new[] { "urbandefine", "urbdef", "def" };
        }

        [Overload (typeof (Embed), "Fetch the definition of a given word from Urban Dictionary.")]
        public async Task<Result> Execute(CommandMetadata _, string word)
        {
            Embed embed = (await UrbanDefinition.Get(word)).ToEmbed ();
            return new Result(embed, string.Empty);
        }
    }
}
