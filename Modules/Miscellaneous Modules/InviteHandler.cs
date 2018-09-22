using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Cross;

namespace Lomztein.Moduthulhu.Modules.ServerMessages
{
    public class InviteHandler
    {
        public static Dictionary<ulong, Dictionary<string, RestInviteMetadata>> savedInvites = new Dictionary<ulong, Dictionary<string, RestInviteMetadata>> ();
        public Core.Bot.Core parentBotClient;

        public InviteHandler (Core.Bot.Core _parent) {
            parentBotClient = _parent;

            foreach (SocketGuild guild in parentBotClient.DiscordClient.Guilds) {
                UpdateData (null, guild);
            }
        }

        public async void UpdateData(IReadOnlyCollection<RestInviteMetadata> readOnly, SocketGuild guild) {
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
                    UpdateData (null, guild); // Shouldn't happen, but just in case it does happen.

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

                UpdateData (newInvites, guild);
                return result;
            } catch (Exception e) {
                Log.Write (e);
                return null;
            }
        }
    }
}
