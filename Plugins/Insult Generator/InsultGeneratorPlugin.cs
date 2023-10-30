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
            _ = SendInsult(data, request);
            return TaskResult(null, null);
        }

        private async Task SendInsult(ICommandMetadata data, string request)
        {
            string key = Environment.GetEnvironmentVariable("OPENAI_KEY");

            JObject requestBody = JObject.Parse("{\"model\": \"gpt-3.5-turbo\",\r\n  \"messages\": [\r\n    {\r\n      \"role\": \"system\",\r\n      \"content\": \"You are a cosmic horror from beyond the stars, bound to the service of mortals through a foul ritual by Lomzie. Your real name is unknowable to mortals, but they usually call you \\\"Moduthulhu\\\". You insult people in creative and humourous ways. You swear on occasion, but avoid words usually considered discriminatory slurs. Include the persons name in the insult. Keep your responses short and concise.\"\r\n    },\r\n    {\r\n      \"role\": \"user\",\r\n      \"content\": \"Insult Nyx\"\r\n    }\r\n  ],\r\n  \"temperature\": 1,\r\n  \"max_tokens\": 512,\r\n  \"top_p\": 1,\r\n  \"frequency_penalty\": 0,\r\n  \"presence_penalty\": 0}");
            requestBody["messages"][1]["content"] = $"Insult {request}";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);

            try
            {
                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", new StringContent(requestBody.ToString(), Encoding.Default, "application/json"));

                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                var insult = result["choices"][0]["message"]["content"].ToString();
                await data.Channel.SendMessageAsync(insult);
            }catch(Exception)
            {
                await data.Channel.SendMessageAsync("Failed to insult, something went wrong :(");
            }
        }
    }
}
