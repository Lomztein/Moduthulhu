using Discord.Net;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    [Descriptor("Moduthulhu", "Consent", "Plugin that allows users to toggle whether or not they consent to personal data storage. Whether or not it is respected is dependant on individual plugins. Use '!plugin info <plugin>' on a plugin to view compliance.", "1.0.0")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/blob/master/Core/Plugin/Standard%20Plugins/ConsentPlugin.cs")]
    [Dependency("Moduthulhu-Command Root")]
    [GDPR(GDPRCompliance.Full)]
    [Critical]
    public class ConsentPlugin : PluginBase
    {
        private ConsentCommand _consentCommand;
        private RequestDataCommand _requestCommand;
        private DeleteDataCommand _deleteCommand;

        public override void Initialize()
        {
            _consentCommand = new ConsentCommand { ParentPlugin = this };
            _requestCommand = new RequestDataCommand { ParentPlugin = this };
            _deleteCommand = new DeleteDataCommand { ParentPlugin = this };

            SendMessage("Moduthulhu-Command Root", "AddCommand", _consentCommand);
            SendMessage("Moduthulhu-Command Root", "AddCommand", _requestCommand);
            SendMessage("Moduthulhu-Command Root", "AddCommand", _deleteCommand);
        }

        public override void Shutdown()
        {
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _consentCommand);
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _requestCommand);
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _deleteCommand);
        }

        public override JToken RequestUserData(ulong id)
        {
            bool consented = Consent.TryAssertConsent(GuildHandler.GuildId, id);
            var obj = new JObject
            {
                { "Consents", consented }
            };
            return obj;
        }

        public override void DeleteUserData(ulong id)
        {
            Consent.DeleteConsent(GuildHandler.GuildId, id);
        }
    }

    public class ConsentCommand : PluginCommand<ConsentPlugin>
    {
        public ConsentCommand()
        {
            Name = "consent";
            Description = "Personal data storage";
            Category = AdditionalCategories.Management;
        }

        [Overload(typeof(void), "Toggle consent for the bot to store personal data.")]
        public Task<Result> Execute(CommandMetadata metadata)
        {
            ulong guildId = (metadata.Author as SocketGuildUser).Guild.Id;
            ulong userId = metadata.AuthorID;
            if (Consent.TryAssertConsent(guildId, userId))
            {
                Consent.SetConsent(guildId, userId, false);
                return TaskResult(null, "You have disabled consent for the bot to store personal data.");
            }
            else
            {
                Consent.SetConsent(guildId, userId, true);
                return TaskResult(null, "You have enabled consent for the bot to store personal data.");
            }
        }
    }

    public class RequestDataCommand : PluginCommand<ConsentPlugin>
    {
        public RequestDataCommand ()
        {
            Name = "requestdata";
            Description = "Request personal data";
            Category = AdditionalCategories.Management;
        }

        [Overload(typeof(void), "Request a JSON file containing any data linked to your Discord ID in this server.")]
        public async Task<Result> Execute(CommandMetadata metadata)
        {
            JObject data = ParentPlugin.GuildHandler.Plugins.RequestUserData(metadata.AuthorID);
            MemoryStream stream = new MemoryStream();

            using (TextWriter writer = new StreamWriter(stream))
            {
                string text = data.ToString();
                writer.WriteLine(text);
                writer.Flush();

                stream.Seek(0, SeekOrigin.Begin);

                try
                {
                    var dm = await metadata.Author.CreateDMChannelAsync();
                    await dm.SendFileAsync(stream, metadata.AuthorID.ToString(CultureInfo.InvariantCulture) + ".json", "Your personal data, as requested. You may delete all of this using the `!deletedata` command in the same server as you requested the data. If we share multiple servers, you must do this for each server. The file may be opened as a text file in something like Notepad. Additionally, a website like http://jsonviewer.stack.hu/ may make reading it easier.");
                }
                catch (HttpException)
                {
                    return new Result(null, "Failed to deliver file, access to your DM is required for delivery.");
                }
            }

            return new Result(null, "A file containing all your personal data has been delivered via DM.");
        }
    }

    public class DeleteDataCommand : PluginCommand<ConsentPlugin>
    {
        public DeleteDataCommand ()
        {
            Name = "deletedata";
            Description = "Delete personal data";
            Category = AdditionalCategories.Management;
        }

        [Overload (typeof (void), "Delete any permanently stored plugin data that is linked to your Discord ID in this server.")]
        public Task<Result> Execute (CommandMetadata metadata)
        {
            QuestionMessage message = new QuestionMessage("Are you sure you wish to delete personal data? This is a permanent action and cannot be undone.", async () => { 
                ParentPlugin.GuildHandler.Plugins.DeleteUserData(metadata.AuthorID);
                await metadata.Message.Channel.SendMessageAsync ("Stored data linked to your ID has succesfully been deleted. Keep in mind some plugins may require to keep track of your ID to function, so they may immidiately store your ID again.");
                }, async () => await metadata.Message.Channel.SendMessageAsync ("Data deletion cancelled."));
            return TaskResult(message, string.Empty);
        }
    }
}
