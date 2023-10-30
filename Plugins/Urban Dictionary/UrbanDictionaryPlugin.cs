using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
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
    [Dependency("Moduthulhu-Command Root")]
    public class UrbanDictionaryPlugin : PluginBase
    {
        private ICommand cmd;

        private CachedValue<bool> _enableHyperlinkReactions;
        public bool EnableHyperlinkReactions => _enableHyperlinkReactions.GetValue();
        public static readonly string[] ReactionEmojis = new string[] { "1️⃣", "2️⃣", "3️⃣", "4️⃣", "5️⃣", "6️⃣", "7️⃣", "8️⃣", "9️⃣" };
        private List<NestedDefinitionButton> _nestedButtons = new List<NestedDefinitionButton>();
        public void AddNestedDefinitionButton(NestedDefinitionButton butt) => _nestedButtons.Add(butt);

        public override void Initialize()
        {
            cmd = new UrbanDefineCommand { ParentPlugin = this };
            SendMessage("Moduthulhu-Command Root", "AddCommand", cmd);
            _enableHyperlinkReactions = GetConfigCache("EnableHyperlinkReactions", x => false);
            AddConfigInfo("Toggle Hyperlink Reactions", "Toggle reactions", () => _enableHyperlinkReactions.SetValue(!_enableHyperlinkReactions.GetValue()), (success) => _enableHyperlinkReactions.GetValue() ? "Hyperlink reactions has been enabled." : "Hyperlink reactions has been disabled.");
            GuildHandler.ReactionAdded += GuildHandler_ReactionAdded;
        }

        private async Task GuildHandler_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, Discord.WebSocket.SocketReaction arg3)
        {
            if (EnableHyperlinkReactions)
            {
                NestedDefinitionButton button = _nestedButtons.FirstOrDefault(x => x.MessageId == arg1.Id);
                if (button != null && !arg3.User.GetValueOrDefault().IsBot)
                {
                    int emoji = ReactionEmojis.ToList().IndexOf(arg3.Emote.Name);
                    if (button.Contains(emoji))
                    {
                        string word = button.Consume(emoji);
                        var def = await UrbanDefinition.Get(word, EnableHyperlinkReactions);
                        await arg2.GetOrDownloadAsync();
                        var msg = await arg2.Value.SendMessageAsync(null, false, def.ToEmbed());
                        await AddNestedDefReactions(def, msg);
                    }
                }
            }
        }

        public override void Shutdown()
        {
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", cmd);
            GuildHandler.ReactionAdded -= GuildHandler_ReactionAdded;
        }

        public async Task AddNestedDefReactions (UrbanDefinition definition, IUserMessage message)
        {
            if (EnableHyperlinkReactions)
            {
                int length = Math.Min(definition.NestedDefinitionWords.Length, ReactionEmojis.Length);
                AddNestedDefinitionButton(new NestedDefinitionButton(message.Id, definition.NestedDefinitionWords));

                for (int i = 0; i < length; i++)
                {
                    await message.AddReactionAsync(new Emoji(ReactionEmojis[i]));
                }
            }
        }
    }

    public class UrbanDefinition
    {
        private static readonly string[] SuperscriptNumbers = new string[] { "¹", "²", "³", "⁴", "⁵", "⁶", "⁷", "⁸", "⁹" };
        public const string ApiUrl = "http://api.urbandictionary.com/v0/define?term={word}";
        public const string SearchUrl = "https://www.urbandictionary.com/define.php?term={word}";

        public readonly string Word;
        public readonly string Definition;
        public readonly string Example;
        public readonly string Author;
        public readonly string Permalink;
        public readonly bool Success;
        private readonly bool _enableHyperlinkReactions;

        private readonly List<string> _nestedDefs = new List<string>();
        public string[] NestedDefinitionWords => _nestedDefs.ToArray();

        public string[] Tags { get; }
        public string[] Sounds { get; }

        readonly int ThumbsUp;
        readonly int ThumbsDown;

        public UrbanDefinition(JObject jObject, string word, bool enableHyperlinkReactions)
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

                _enableHyperlinkReactions = enableHyperlinkReactions;
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
                    .WithDescription(Cutoff(EscapeEmphasis(EmbedNestedDefinitions(Definition)), 2048, "... (See source for more)"))
                    .WithFooter($"Defined by {Author}. Souce: www.urbandictionary.com");
                if (Example.Length > 0)
                {
                    builder.AddField("Example", Cutoff ("> " + EscapeEmphasis (string.Join("\n> ", EmbedNestedDefinitions(Example).Split('\n'))), 1024, "... (See source for more)"));
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
            string result = squareBracketRegex.Replace(input, x => $"{x.Value}({GetSearchUrl(x.Value.Substring(1, x.Value.Length - 2))}){AddNestedDef(x.Value)}");
            return result;
        }

        private string AddNestedDef (string url)
        {
            if (_nestedDefs.Count == UrbanDictionaryPlugin.ReactionEmojis.Length || !_enableHyperlinkReactions)
            {
                return string.Empty;
            }

            if (!_nestedDefs.Contains (url))
            {
                _nestedDefs.Add(url);
            }
            return SuperscriptNumbers[_nestedDefs.IndexOf(url)];
        }

        private string EscapeEmphasis (string input)
        {
            Regex toEscapeRegex = new Regex(@"_|\*");
            return toEscapeRegex.Replace(input, x => $"\\{x.Value}");
        }

        private string Cutoff (string input, int maxChars, string trail)
        {
            int max = maxChars - trail.Length;
            if (input.Length > max)
            {
                string cutoff = input.Substring(0, max) + trail;
                return cutoff;
            }
            return input;
        }

        private string GetSearchUrl(string word) => SearchUrl.Replace("{word}", word).Replace (" ", "%20");

        public static async Task<UrbanDefinition> Get(string word, bool enableHyperlinkReactions)
        {
            try
            {
                JObject json = await HTTP.GetJSON(new Uri(ApiUrl.Replace("{word}", word))).ConfigureAwait(false);
                return new UrbanDefinition(json, word, enableHyperlinkReactions);
            } catch (InvalidOperationException)
            {
                throw new InvalidOperationException("Unable to fetch definition. The service may be temporarily unavailable.");
            }
        }
    }

    public class NestedDefinitionButton
    {
        public ulong MessageId { get; private set; }
        private List<string> _nestedDefs;

        public NestedDefinitionButton(ulong messageId, IEnumerable<string> nestedDefs)
        {
            MessageId = messageId;
            _nestedDefs = new List<string>(nestedDefs);
        }

        public bool Contains(int index) => _nestedDefs[index] != null;

        public string Consume (int index)
        {
            string word = _nestedDefs[index];
            _nestedDefs[index] = null;
            return word;
        }
    }

    public class UrbanDefineCommand : PluginCommand<UrbanDictionaryPlugin>
    {
        public UrbanDefineCommand()
        {
            Name = "define";
            Description = "Understand linguistics.";
            Category = StandardCategories.Fun;
            Aliases = new[] { "urbandefine", "urbdef", "def" };
        }

        [Overload (typeof (Embed), "Fetch the definition of a given word from Urban Dictionary.")]
        public async Task<Result> Execute(ICommandMetadata data, string word)
        {
            var def = await UrbanDefinition.Get(word, ParentPlugin.EnableHyperlinkReactions);
            Embed embed = def.ToEmbed ();

            var message = await data.Channel.SendMessageAsync(null, false, embed);
            await ParentPlugin.AddNestedDefReactions(def, message);

            return new Result(null, string.Empty);
        }
    }
}
