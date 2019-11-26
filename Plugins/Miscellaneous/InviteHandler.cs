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
        public static Dictionary<ulong, Dictionary<string, RestInviteMetadata>> savedInvites = new Dictionary<ulong, Dictionary<string, RestInviteMetadata>> ();
        public GuildHandler _guildHandler;

        public InviteHandler (GuildHandler guildHandler) {
            _guildHandler = guildHandler;
        }

        public async Task Intialize()
        {
            await UpdateData(null, _guildHandler.GetGuild ());
        }

        public async Task UpdateData(IReadOnlyCollection<RestInviteMetadata> readOnly, SocketGuild guild) {
            try {
                IReadOnlyCollection<RestInviteMetadata> invites = null;
                if (readOnly == null)
                {
                    invites = await guild.GetInvitesAsync();
                }

                if (!savedInvites.ContainsKey (guild.Id))
                {
                    savedInvites.Add(guild.Id, new Dictionary<string, RestInviteMetadata>());
                }

                savedInvites[guild.Id] = invites.ToDictionary (x => x.Code);
            } catch (Exception e) {
                Log.Write (e);
            }
        }

        public async Task<RestInviteMetadata> FindInviter(SocketGuild guild) {
            try {
                IReadOnlyCollection<RestInviteMetadata> newInvites = await guild.GetInvitesAsync ();
                Dictionary<string, RestInviteMetadata> dict = newInvites.ToDictionary (x => x.Code);
                RestInviteMetadata result = null;

                if (!savedInvites.ContainsKey (guild.Id))
                    await UpdateData (null, guild); // Shouldn't happen, but just in case it does happen.

                foreach (var key in dict) {
                    if (savedInvites[guild.Id].ContainsKey (key.Key)) {
                        if (savedInvites [ guild.Id ] [ key.Key ].Uses + 1 == key.Value.Uses) {
                            result = key.Value;
                        }
                    } else {
                        if (key.Value.Uses == 1)
                        {
                            result = key.Value;
                        }
                    }
                }

                await UpdateData (newInvites, guild);
                return result;
            } catch (Exception e) {
                Log.Write (e);
                return null;
            }
        }
    }
}
