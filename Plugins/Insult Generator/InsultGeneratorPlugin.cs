using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.InsultGenerators;
using Lomztein.Moduthulhu.Plugins.Standard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins
{
    [Descriptor ("Lomztein", "Insult Generator", "Ever wanted to rudely insult someone, but you are completely lacking any sense of creativity what so ever? Well I've got the solution for you!")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Plugins/Insult%20Generator")]
    public class InsultGeneratorPlugin : PluginBase
    {
        private IInsultGenerator[] _generators;

        private ICommand _cmd;

        public override void Initialize()
        {
            _cmd = new InsultCommand { ParentPlugin = this };
            SendMessage("Moduthulhu-Command Root", "AddCommand", _cmd);
            InitHandlers();
        }

        private void InitHandlers()
        {
            _generators = new IInsultGenerator[]
            {
                new InsultGenerator (GuildHandler, Path.Combine (BotCore.ResourcesDirectory, "InsultData")),
                // Add new insult generators here.
            };
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
            Aliases = new[] { "fuck" };
        }

        [Overload (typeof (string), "Insult yourself, dummy.")]
        public Task<Result> Execute (CommandMetadata data)
        {
            return Insult(data.Author.GetShownName());
        }

        [Overload(typeof(string), "Insult someone specific, you ass.")]
        public Task<Result> Execute(CommandMetadata _, SocketGuildUser user)
        {
            return Insult (user.GetShownName());
        }

        [Overload(typeof(string), "Insult someone specific, you butt.")]
        public Task<Result> Execute(CommandMetadata data, string user)
        {
            SocketGuildUser userObj = ParentPlugin.GuildHandler.FindUser(user);
            if (userObj != null)
            {
                return Insult (ParentPlugin.GuildHandler.GetUser(user).GetShownName());
            }
            else
            {
                return Insult (user);
            }
        }

        [Overload(typeof(string), "Insult someone specific, you donkey.")]
        public Task<Result> Execute(CommandMetadata data, ulong user)
        {
            return Insult (ParentPlugin.GuildHandler.GetUser(user).GetShownName());
        }

        private Task<Result> Insult (string user)
        {
            string insult = ParentPlugin.SelectGenerator().Insult(user);
            return TaskResult(insult, insult);
        }
    }
}
