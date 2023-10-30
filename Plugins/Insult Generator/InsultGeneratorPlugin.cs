using Discord.Net;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.InsultGenerators;
using Lomztein.Moduthulhu.Plugins.Standard;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins
{
    [Descriptor ("Lomztein", "Insult Generator", "Ever wanted to rudely insult someone, but you are completely lacking any sense of creativity what so ever? Well I've got the solution for you!")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Plugins/Insult%20Generator")]
    [Dependency("Lomztein-OpenAI")]
    public class InsultGeneratorPlugin : PluginBase
    {
        private IInsultGenerator[] _generators;

        private ICommand[] _cmds;

        public override void Initialize()
        {
            _cmds = new ICommand[] {
                new InsultCommand_Legacy { ParentPlugin = this },
                new InsultCommand { ParentPlugin = this },
                };

            SendMessage("Moduthulhu-Command Root", "AddCommands", _cmds);
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
            SendMessage("Moduthulhu-Command Root", "RemoveCommands", _cmds);
        }

        public static IInsultGenerator SelectGenerator (IEnumerable<IInsultGenerator> options)
        {
            Random random = new Random();
            var list = options.ToList();
            return list[random.Next(0, list.Count)];
        }

        public IInsultGenerator SelectGenerator() => SelectGenerator(_generators);

        public async Task SendOpenAIInsult(ICommandMetadata data, string request)
        {
            var response = await SendMessage<Task<string>>("Lomztein-OpenAI", "GetChatResponseAsync_DefaultProfileBase", "You insult people in creative and humourous ways. Include the named person in the response.", request);
            await data.Channel.SendMessageAsync(response);
        }
    }

    public class InsultCommand_Legacy : PluginCommand<InsultGeneratorPlugin>
    {
        public InsultCommand_Legacy ()
        {
            Name = "insult-legacy";
            Description = "So rude >:(";
            Category = StandardCategories.Fun;
            Aliases = new[] { "fuck-legacy" };
        }

        [Overload (typeof (string), "Insult yourself, dummy.")]
        public Task<Result> Execute (ICommandMetadata data)
        {
            return Insult(data.Author.GetShownName());
        }

        [Overload(typeof(string), "Insult someone specific, you ass.")]
        public Task<Result> Execute(ICommandMetadata _, SocketGuildUser user)
        {
            return Insult (user.GetShownName());
        }

        [Overload(typeof(string), "Insult someone specific, you butt.")]
        public Task<Result> Execute(ICommandMetadata data, string user)
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
        public Task<Result> Execute(ICommandMetadata data, ulong user)
        {
            return Insult (ParentPlugin.GuildHandler.GetUser(user).GetShownName());
        }

        private Task<Result> Insult (string user)
        {
            string insult = ParentPlugin.SelectGenerator().Insult(user);
            return TaskResult(insult, insult);
        }
    }

    public class InsultCommand : PluginCommand<InsultGeneratorPlugin>
    {
        public InsultCommand()
        {
            Name = "insult";
            Description = "Now using OpenAI!";
            Category = StandardCategories.Fun;
            Aliases = new[] { "fuck" };
        }

        [Overload(typeof(string), "Insult someone using the power of AI!")]
        public Task<Result> Execute(ICommandMetadata data, string request)
        {
            _ = ParentPlugin.SendOpenAIInsult(data, request);
            return TaskResult(null, null);
        }
    }
}
