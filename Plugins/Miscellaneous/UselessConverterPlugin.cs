using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Miscellaneous
{
    [Descriptor("Lomztein", "Useless Converter", "Allows the conversion of values to other values.")]
    public class UselessConverterPlugin : PluginBase
    {
        private enum UnitType { Length, Speed, Density }

        private List<Converter> _converters = new List<Converter>(); // TODO: Add support for custom converters.
        private CachedValue<double> _chanceToAutoConvert;
        private Command _command;

        public override void Initialize()
        {
            InitConverters(_converters);
            _chanceToAutoConvert = GetConfigCache("ChanceToAutoConvert", x => 1d);
            AddConfigInfo<double>("chancetoautoconvert", "Auto conversion chance", x => _chanceToAutoConvert.SetValue(x), (success, chance) => "Auto conversion chance set to " + chance, "chance");
            GuildHandler.MessageReceived += OnMessageRecieved;

            _command = new ConvertCommand() { ParentPlugin = this };
            SendMessage("Moduthulhu-Command Root", "AddCommand", _command);
        }

        private async Task OnMessageRecieved(SocketMessage arg)
        {
            Random random = new Random();
            if (!arg.Author.IsBot)
            {
                foreach (Converter converter in _converters)
                {
                    Regex regex = new Regex(@"((\d+[,.]?\d*)(\s+)(" + converter.Name.ToLower() + "s?))");
                    Match match = regex.Match(arg.Content.ToLower());
                    if (match.Success && match.Groups.Count >= 3)
                    {
                        if (random.NextDouble() * 100 < _chanceToAutoConvert.GetValue())
                        {
                            Group group1 = match.Groups[match.Groups.Count - 3];
                            string fuckCommas = group1.Value.Replace(',', '.');

                            if (double.TryParse(fuckCommas, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                            {
                                string result = GetConversionStringRandom(number, converter);
                                await arg.Channel.SendMessageAsync(result);
                            }
                        }
                    }
                }
            }
        }

        public override void Shutdown()
        {
            GuildHandler.MessageReceived -= OnMessageRecieved;
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _command);
            ClearConfigInfos();
        }

        private Converter FindConverter(string name) => _converters.FirstOrDefault(x => x.Name == name);

        private Converter FindRandomOfType(UnitType type, Converter not)
        {
            Random random = new Random();
            Converter[] converters = _converters.Where(x => x.Type == type && x != not).ToArray();
            return converters[random.Next(0, converters.Length)];
        }

        private string GetConversionStringRandom(double value, Converter from)
        {
            Converter random = FindRandomOfType(from.Type, from);
            return GetConversionString(value, from, random);
        }

        private string GetConversionString(double value, Converter from, Converter to)
        {
            double si = from.ToSI(value);
            string converted = string.Format("{0:0.####}", to.FromSI(si));
            return $"{value} {from.Name}s is equal to {converted} {to.Name}s.";
        }

        private class Converter
        {
            public UnitType Type { get; private set; }
            public string Name { get; private set; }

            public Func<double, double> ToSI { get; private set; }
            public Func<double, double> FromSI { get; private set; }

            public Converter(UnitType type, string name, Func<double, double> toSI, Func<double, double> fromSI)
            {
                Type = type;
                Name = name;
                ToSI = toSI;
                FromSI = fromSI;
            }
        }

        public class ConvertCommand : PluginCommand<UselessConverterPlugin>
        {
            public ConvertCommand ()
            {
                Name = "convert";
                Description = "Convert from one unit to another.";
                Category = StandardCategories.Utility;
            }

            [Overload(typeof(double), "Convert from one specified unit to another specified unit")]
            public Task<Result> Execute (CommandMetadata metadata, double value, string from, string to)
            {
                Converter fromConverter = ParentPlugin.FindConverter(from);
                Converter toConverter = ParentPlugin.FindConverter(to);

                if (fromConverter != null && toConverter != null)
                {
                    value = toConverter.FromSI(toConverter.ToSI(value));
                    return TaskResult(value, ParentPlugin.GetConversionString(value, fromConverter, toConverter));
                }
                else if (fromConverter == null)
                {
                    throw new InvalidOperationException("Cannot convert from unit type " + from);
                }else if (toConverter == null)
                {
                    throw new InvalidOperationException("Cannot convert to unit type " + to);
                }
                else
                {
                    throw new InvalidOperationException("You shouldn't be able to get this error message, if you do I'll give you a hug.");
                }
            }

            [Overload(typeof(double), "Convert from a specified unit to a random unit")]
            public Task<Result> Execute (CommandMetadata metadata, double value, string from)
            {
                Converter fromConverter = ParentPlugin.FindConverter(from);
                if (fromConverter != null)
                {
                    Converter toConverter = ParentPlugin.FindRandomOfType(fromConverter.Type, fromConverter);
                    return Execute(metadata, value, from, toConverter.Name); // don't question it.
                }
                else
                {
                    throw new InvalidOperationException("Cannot convert from unit type " + from);
                }
            }
        }

        private void InitConverters (List<Converter> converters)
        {
            converters.Add(new Converter(UnitType.Length, "meter", x => x, x => x));
            converters.Add(new Converter(UnitType.Length, "freedom unit", x => x * 0.3048, x => x / 0.3048));
            converters.Add(new Converter(UnitType.Length, "american football field", x => x * 91.44, x => x / 91.44));
            converters.Add(new Converter(UnitType.Length, "globally avarage penis", x => x * 0.129, x => x / 0.129));
            converters.Add(new Converter(UnitType.Length, "M1911", x => x * 0.216, x => x / 0.216));
            converters.Add(new Converter(UnitType.Length, "Mikoyan MiG-29", x => x * 17.32, x => x / 17.32));
            converters.Add(new Converter(UnitType.Length, "Jack Black", x => x * 1.68, x => x / 1.68));
            converters.Add(new Converter(UnitType.Length, "marathon", x => x * 42195, x => x / 42195));
            converters.Add(new Converter(UnitType.Length, "international space station", x => x * 109, x => x / 109));
            converters.Add(new Converter(UnitType.Length, "A3 paper heights", x => x * 0.420, x => x / 0.42));
            converters.Add(new Converter(UnitType.Length, "hungarian men", x => x * 1.7914, x => x / 1.7914));
            converters.Add(new Converter(UnitType.Length, "fully operational battlecruiser", x => x * 9000, x => x / 9000));
            converters.Add(new Converter(UnitType.Length, "Samsung WD82T4047CE washing machine with wifi support", x => x * 0.60, x => x / 0.60));
            converters.Add(new Converter(UnitType.Length, "Severin MW 7893 Red microwave plate", x => x * 0.254, x => x / 0.254));
            converters.Add(new Converter(UnitType.Length, "Take bad dragon tentacle dildo", x => x * 0.2095, x => x / 0.2095));
            converters.Add(new Converter(UnitType.Length, "Duke's Muzzle bad dragon fleshlight", x => x * 0.2032, x => x / 0.2032));
            converters.Add(new Converter(UnitType.Length, "avarage distance between the sun and the earth", x => x * 150000000, x => x / 150000000));
        }
    }
}
