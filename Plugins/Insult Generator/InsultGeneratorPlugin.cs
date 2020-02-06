using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.InsultGenerators;
using Lomztein.Moduthulhu.Plugins.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins
{
    [Descriptor ("Lomztein", "Insult Generator", "Ever wanted to rudely insult someone, but you are completely lacking any sense of creativity what so ever? Well I've got the solution for you!")]
    public class InsultGeneratorPlugin : PluginBase
    {
        private IInsultGenerator[] _generators = new IInsultGenerator[]
        {
            new AdjectiveVerbNounInsultGenerator (),
        };

        private ICommand _cmd;

        public override void Initialize()
        {
            _cmd = new InsultCommand { ParentPlugin = this };
            SendMessage("Moduthulhu-Command Root", "AddCommand", _cmd);
        }

        public override void Shutdown()
        {
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _cmd);
        }

        public static IInsultGenerator SelectGenerator (IEnumerable<IInsultGenerator> options)
        {
            Random random = new Random();
            var list = options.ToList();
            return list[random.Next(0, list.Count)];
        }

        public IInsultGenerator SelectGenerator() => SelectGenerator(_generators);
    }

    public class InsultCommand : PluginCommand<InsultGeneratorPlugin>
    {
        public InsultCommand ()
        {
            Name = "insult";
            Description = "So rude >:(";
            Category = StandardCategories.Fun;
        }

        [Overload (typeof (void), "Insult yourself, dummy.")]
        public Task<Result> Execute (CommandMetadata data)
        {
            return TaskResult(null, ParentPlugin.SelectGenerator().Insult(data.Author.GetShownName()));
        }

        [Overload(typeof(void), "Insult someone specific, you ass.")]
        public Task<Result> Execute(CommandMetadata _, SocketGuildUser user)
        {
            return TaskResult(null, ParentPlugin.SelectGenerator().Insult(user.GetShownName()));
        }

        [Overload(typeof(void), "Insult someone specific, you butt.")]
        public Task<Result> Execute(CommandMetadata data, string user)
        {
            SocketGuildUser userObj = ParentPlugin.GuildHandler.FindUser(user);
            if (userObj != null)
            {
                return Execute(data, ParentPlugin.GuildHandler.GetUser(user));
            }
            else
            {
                return TaskResult(null, ParentPlugin.SelectGenerator().Insult(user));
            }
        }

        [Overload(typeof(void), "Insult someone specific, you donkey.")]
        public Task<Result> Execute(CommandMetadata data, ulong user)
        {
            return Execute(data, ParentPlugin.GuildHandler.GetUser(user));
        }
    }
}
