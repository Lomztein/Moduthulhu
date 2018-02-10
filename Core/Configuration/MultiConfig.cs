using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Configuration
{
    /// <summary>
    /// This is designed to be easy to use for multiserver modules.
    /// </summary>
    public class MultiConfig : Config {

        public MultiConfig (string _name) : base (_name) { }

        public T GetEntry<T> (SocketGuild guild, string key, T fallback) {
            return GetEntry (guild.Id, key, fallback);
        }

        public MultiEntry<T> GetEntries<T> (IEnumerable<IEntity<ulong>> entities, string key, T fallback) {
            Dictionary<ulong, T> entries = new Dictionary<ulong, T> ();
            foreach (SocketGuild entity in entities) {
                entries.Add (entity.Id, GetEntry (entity, key, fallback));
            }
            return new MultiEntry<T> (entries);
        }
    }
}
