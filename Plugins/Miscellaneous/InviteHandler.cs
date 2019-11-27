using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core;

namespace Lomztein.Moduthulhu.Modules.ServerMessages
{
    public class InviteHandler
    {
        private Dictionary<string, RestInviteMetadata> savedInvites = new Dictionary<string, RestInviteMetadata> ();
        private readonly GuildHandler _guildHandler;

        public InviteHandler (GuildHandler guildHandler) {
            _guildHandler = guildHandler;
        }

        public async Task Intialize()
        {
            await UpdateData(_guildHandler.GetGuild ());
        }

        public async Task UpdateData(SocketGuild guild) => savedInvites = (await guild.GetInvitesAsync()).ToDictionary(x => x.Code);

        public async Task<RestInviteMetadata> FindInviter(SocketGuild guild) {
            try {
                IReadOnlyCollection<RestInviteMetadata> newInvites = await guild.GetInvitesAsync ();
                Dictionary<string, RestInviteMetadata> dict = newInvites.ToDictionary (x => x.Code);
                RestInviteMetadata result = null;

                foreach (var key in dict) {
                    if (savedInvites.ContainsKey (key.Key)) {
                        if (savedInvites[ key.Key ].Uses + 1 == key.Value.Uses) {
                            result = key.Value;
                        }
                    } else {
                        if (key.Value.Uses == 1)
                        {
                            result = key.Value;
                        }
                    }
                }

                await UpdateData (guild);
                return result;
            } catch (Exception e) {
                Log.Exception(e);
                return null;
            }
        }
    }
}
