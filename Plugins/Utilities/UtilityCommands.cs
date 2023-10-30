using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Standard.Utilities
{
    public class RollTheDice : Command
    {
        public RollTheDice ()
        {
            Name = "rtd";
            Description = "Roll the Dice";
            Category = StandardCategories.Utility;
        }

        [Overload (typeof (int), "Roll a six sided die and get the result.")]
        public Task<Result> Execute (ICommandMetadata metadata)
        {
            return Execute(metadata, 6);
        }

        [Overload (typeof (int), "Roll an n-sided die and get the result.")]
        public Task<Result> Execute (ICommandMetadata _, int sides)
        {
            Random random = new Random();
            int value = random.Next(1, sides + 1);
            return TaskResult (value, $"You rolled a {value}!");
        }
    }

    public class FlipCoin : Command
    {
        public FlipCoin ()
        {
            Name = "flipcoin";
            Description = "Flip a coin!";
            Category = StandardCategories.Utility;
            Aliases = new [] { "coinflip" };
        }

        [Overload (typeof (int), "Flip a coin and recieve either Heads or Tails!")]
        public Task<Result> Execute (ICommandMetadata _)
        {
            string result = new Random().Next(0, 2) == 0 ? "Heads" : "Tails";
            return TaskResult(result, $"The coin landed on {result}!");
        }
    }

    public class Embolden : Command
    {
        private char[] available = new char[] { 'a', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't'
                                              , 'u', 'v', 'w', 'x', 'y', 'z' };

        private char[] numbers = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private Dictionary<char, string> specifics = new Dictionary<char, string>();
        private bool ignoreUnavailable = true;

        public Embolden ()
        {
            Name = "embolden";
            Description = "Embrace the Bold";
            Category = StandardCategories.Fun;
            specifics.Add('b', "🅱");
        }

        [Overload (typeof (string), "Embolden the given piece of text.")]
        public Task<Result> Execute(ICommandMetadata _, string input)
        {
            StringBuilder outText = new StringBuilder ();

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ' ')
                {
                    outText.Append ("  ");
                }
                else
                {
                    char letter = input.ToLower()[i];
                    if (available.Contains(letter))
                    {
                        outText.Append(":regional_indicator_" + input.ToLower()[i] + ": ");
                    }
                    else if (specifics.ContainsKey(letter))
                    {
                        outText.Append (specifics[letter]);
                    }
                    else if (numbers.Contains(letter))
                    {
                        outText.Append (NumberToString(letter) + " ");
                    }
                    else if (!ignoreUnavailable)
                    {
                        return TaskResult("", "Unavailable character detected: " + letter);
                    }
                }
            }

            return TaskResult(outText.ToString (), outText.ToString ());
        }

        // Considering this is now used in more than one class, it might be wise to move it to a core class in order to remain structured.
        private static string NumberToString(char number)
        {
            switch (number)
            {
                case '0':
                    return ":zero:";

                case '1':
                    return ":one:";

                case '2':
                    return ":two:";

                case '3':
                    return ":three:";

                case '4':
                    return ":four:";

                case '5':
                    return ":five:";

                case '6':
                    return ":six:";

                case '7':
                    return ":seven:";

                case '8':
                    return ":eight:";

                case '9':
                    return ":nine:";
            }

            return "";
        }
    }

    public class Fizzfyr13 : Command
    {
        public Fizzfyr13 ()
        {
            Name = "fizzfyr13";
            Description = "Raw sexual energy";
            Category = StandardCategories.Fun;
            Aliases = new [] { "sexmachine", "guffesbodypillow" };
        }

        [Overload (typeof (string), "Witness the true power of the moist side.")]
        public Task<Result> Execute (ICommandMetadata metadata)
        {
            metadata.Channel.SendFileAsync(BotCore.ResourcesDirectory + "/fizzfyr.jpg", "Looking for this sexy stud?");
            return TaskResult("I'm so sorry you had to witness this.", null);
        }
    }

    public class Ping : PluginCommand<UtilitiesPlugin>
    {
        public Ping ()
        {
            Name = "ping";
            Description = "Ping the bot.";
            Category = StandardCategories.Utility;
        }

        [Overload (typeof (int), "Ping the bot. Returns ping time in milliseconds.")]
        public Task<Result> Execute (ICommandMetadata _)
        {
            return TaskResult(ParentPlugin.GuildHandler.Shard.Client.Latency, "Pong!");
        }
    }

    public class OwO : PluginCommand<UtilitiesPlugin>
    {
        public OwO()
        {
            Name = "owo";
            Description = "Pwomote hatwad";
            Category = StandardCategories.Fun;
            Aliases = new[] { "uwu" };
        }

        private readonly Regex _tow = new Regex("(?:r|l)");
        private readonly Regex _toW = new Regex("(?:R|L)");
        private readonly Regex _ove = new Regex("ove");
        private readonly Regex _OVE = new Regex("OVE");
        private readonly Regex _ck = new Regex("ck");
        private readonly Regex _CK = new Regex("CK");

        private readonly Regex exclamation = new Regex("(?:!)");

        private readonly string[] faces = new[] { "(・`ω´・)", ";;w;;", "owo", "UwU", ">w<", "^w^" };
        private readonly Regex insertw = new Regex("(?: [bdfgkmpqrstvxz][aeiou])");
        private readonly Regex insertW = new Regex("(?: [BDFGLMPQRSTVXZ][AEIOU])");

        [Overload(typeof(string), "Implementation based heavily on the [OWOifier by @Black_is_Black](https://repl.it/@Black_is_Back/OWOifier#index.js).")]
        public Task<Result> Execute(ICommandMetadata metadata, string input)
        {
            input = _tow.Replace(input, "w");
            input = _toW.Replace(input, "W");
            input = _ove.Replace(input, "uv");
            input = _OVE.Replace(input, "UV");
            input = _ck.Replace(input, "k");
            input = _CK.Replace(input, "K");

            Random random = new Random();
            input = insertw.Replace(input, x => x.Value.Insert(2, "w"));
            input = insertW.Replace(input, x => x.Value.Insert(2, "W"));

            input = exclamation.Replace(input, x => $"! {faces[random.Next(faces.Length)]}");

            return TaskResult(input, input);
        }
    }
}
