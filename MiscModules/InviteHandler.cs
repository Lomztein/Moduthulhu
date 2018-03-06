using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Bot;

namespace Lomztein.Moduthulhu.Modules.ServerMessages
{
    public class InviteHandler
    {
        public static Dictionary<string, RestInviteMetadata> savedInvites;
        public BotClient parentBotClient;

        public InviteHandler (BotClient _parent) {
            parentBotClient = _parent;

            foreach (SocketGuild guild in parentBotClient.discordClient.Guilds) {
                UpdateData (null, guild);
            }
        }

        public async void UpdateData(IReadOnlyCollection<RestInviteMetadata> readOnly, SocketGuild guild) {
            try {
                await parentBotClient.AwaitFullBoot ();
                if (readOnly == null)
                    readOnly = await guild.GetInvitesAsync ();

                savedInvites = readOnly.ToDictionary (x => x.Code);
            } catch (Exception e) {
                Log.Write (e);
            }
        }

        public async Task<RestInviteMetadata> FindInviter(SocketGuild guild) {
            try {
                IReadOnlyCollection<RestInviteMetadata> newInvites = await guild.GetInvitesAsync ();
                Dictionary<string, RestInviteMetadata> dict = newInvites.ToDictionary (x => x.Code);
                RestInviteMetadata result = null;

                foreach (var key in dict) {
                    if (savedInvites.ContainsKey (key.Key)) {
                        if (savedInvites [ key.Key ].Uses + 1 == key.Value.Uses) {
                            result = key.Value;
                        }
                    } else {
                        if (key.Value.Uses == 1)
                            result = key.Value;
                    }
                }

                UpdateData (newInvites, guild);
                return result;
            } catch (Exception e) {
                Log.Write (e);
                return null;
            }
        }
    }
}
