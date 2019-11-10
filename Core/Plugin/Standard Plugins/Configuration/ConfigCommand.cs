using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    public class ConfigCommandSet : PluginCommandSet<ConfigurationPlugin>
    {
        public ConfigCommandSet ()
        {
            Name = "config";
            Description = "Configure bot.";
            Category = AdditionalCategories.Management;
        }
    }

    public class ConfigCommand : Command
    {
        private ConfigFunctionInfo[] _sources;

        public ConfigCommand (ConfigFunctionInfo[] sources, string categoryName, string categoryDesc)
        {
            _sources = sources;
            var first = sources.First();

            Name = Regex.Replace(first.Name.ToLowerInvariant(), "\\s", "");
            Description = first.Desc;
            Category = new Category (categoryName, categoryDesc);
        }

        public override CommandOverload[] GetOverloads()
        {
            CommandOverload[] overloads = new CommandOverload[_sources.Length];
            for (int i = 0; i < overloads.Length; i++)
            {
                ConfigFunctionInfo source = _sources[i];
                overloads[i] = new CommandOverload(
                    typeof(void),
                    source.GetParameters().Select(x => new CommandOverload.Parameter(x.Name, x.Type, Array.Empty<Attribute>())).ToArray(),
                    source.Name,
                    new CommandOverload.ExampleInfo (null, null, null),
                    x =>
                    {
                        var args = new List<object>(x);
                        args.RemoveAt(0); // Remove command metadata.

                        source.Action.DynamicInvoke (args.ToArray ());
                        return TaskResult(null, $"Succesfully altered configuration.");
                    });
            }
            return overloads;
        }

        private void ExecuteSource (int index, object[] args)
        {
            _sources[index].Action.DynamicInvoke(args);
        }
    }
}
