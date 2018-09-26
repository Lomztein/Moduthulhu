using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.WebSocket;

namespace Lomztein.Moduthulhu.Core.Bot
{
    internal class ErrorReporter
    {
        private Core ParentCore { get; set; }
        private UserList ReportTo { get; set; }

        internal ErrorReporter (Core core, string adminFilePath) {
            ParentCore = core;
            ReportTo = new UserList (adminFilePath);
        }

        internal async void ReportError (Exception exception) {

            string message = (exception.Message + " - " + exception.StackTrace).Substring (0, 1900);

            foreach (ulong id in ReportTo.Users) {
                SocketGuild withUser = ParentCore.ClientManager.AllGuilds.FirstOrDefault (x => x.Users.Any (y => y.Id == id));
                SocketGuildUser user = withUser.GetUser (id);
                await (await user.GetOrCreateDMChannelAsync ()).SendMessageAsync (message);
            }

        }
    }
}
