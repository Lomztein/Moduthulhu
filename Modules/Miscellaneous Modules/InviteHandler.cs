using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Cross;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;

namespace Lomztein.Moduthulhu.Modules.ServerMessages
{
    public class InviteHandler
    {
        public static Dictionary<ulong, Dictionary<string, RestInviteMetadata>> savedInvites = new Dictionary<ulong, Dictionary<string, RestInviteMetadata>> ();
        public Shard ParentShard;

        public InviteHandler (Shard shard) {
            ParentShard = shard;
        }

        public async Task Intialize()
        {
            foreach (SocketGuild guild in ParentShard.Guilds)
            {
                await UpdateData(null, guild);
            }
        }

        public async Task UpdateData(IReadOnlyCollection<RestInviteMetadata> readOnly, SocketGuild guild) {
            try {
                if (readOnly == null)
                    readOnly = await guild.GetInvitesAsync ();

                if (!savedInvites.ContainsKey (guild.Id))
                    savedInvites.Add (guild.Id, new Dictionary<string, RestInviteMetadata> ());

                savedInvites[guild.Id] = readOnly.ToDictionary (x => x.Code);
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
                            result = key.Value;
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
