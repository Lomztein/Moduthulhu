using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    [Descriptor("Lomztein", "Consent", "Plugin that allows users to toggle whether or not they consent to personal data storage.", "1.0.0")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu")]
    [Dependency("Lomztein-Command Root")]
    [Critical]
    public class ConsentPlugin : PluginBase
    {
        private ConsentCommand _consentCommand;

        public override void Initialize()
        {
            _consentCommand = new ConsentCommand { ParentPlugin = this };
            SendMessage("Lomztein-Command Root", "AddCommand", _consentCommand);
        }

        public override void Shutdown()
        {
            SendMessage("Lomztein-Command Root", "RemoveCommand", _consentCommand);
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
}
