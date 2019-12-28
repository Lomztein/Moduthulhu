using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    /// <summary>
    /// Simple container class for a list of channels commonly found on guilds.
    /// </summary>
    public class CommonChannels
    {
        private GuildHandler _handler;
        private Dictionary<string, CachedValue<ulong?>> _channelIds;

        /// <summary>
        /// Get the common channel ID for a specific <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">The identifier of a common channel, eg. "general"</param>
        /// <returns></returns>
        public T Get<T> (string identifier) where T : IGuildChannel
        {
            if (Contains(identifier))
            {
                ulong value = _channelIds[identifier].GetValue().GetValueOrDefault();
                return (T)_handler.FindChannel (value);
            }
            return default;
        }

        public bool Contains(string identifier) => _channelIds.ContainsKey(identifier);

        public CommonChannels Add (string identifier, params string[] commonNames)
        {
            _channelIds.Add(identifier, GetCachedId(identifier, commonNames));
            return this;
        }

        private CachedValue<ulong?> GetCachedId(string identifier, string[] commonNames) =>
            new CachedValue<ulong?>(new DoubleKeyJsonRepository("pluginconfig"), _handler.GuildId, $"CommonChannel{identifier}", () => _handler.GetGuild().Channels.FirstOrDefault(x => commonNames.Contains(x.Name.ToUpperInvariant()))?.Id);

    }
}
