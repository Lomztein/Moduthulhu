using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding
{
    public class GuildHandler
    {
        private Shard _shard;
        private ulong _guildId;

        public GuildHandler (Shard shard, ulong guildId)
        {
            _shard = shard;
            _guildId = guildId;
        }
        public SocketGuild GetGuild() => _shard.GetGuild(_guildId);
        public SocketGuildUser GetUser(ulong userId) => GetGuild()?.GetUser(userId);
        public SocketGuildChannel GetChannel(ulong channelId) => GetGuild()?.GetChannel(channelId);
        public SocketTextChannel GetTextChannel(ulong channelId) => _shard.GetChannel(_guildId, channelId) as SocketTextChannel;
        public SocketVoiceChannel GetVoiceChannel(ulong channelId) => _shard.GetChannel(_guildId, channelId) as SocketVoiceChannel;
        public SocketCategoryChannel GetCategoryChannel(ulong channelId) => _shard.GetChannel(_guildId, channelId) as SocketCategoryChannel;
        public SocketRole GetRole(ulong roleId) => GetGuild()?.GetRole(roleId);

    }
}
