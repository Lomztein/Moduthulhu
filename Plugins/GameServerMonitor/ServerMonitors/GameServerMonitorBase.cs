using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.GameServerMonitor.ServerMonitors
{
    public abstract class GameServerMonitorBase : IGameServerMonitor
    {
        protected abstract string[] SupportedGames { get; }

        public bool CanMonitorGame(string gameName)
            => SupportedGames.Any(x => x.ToLowerInvariant().Equals(gameName.ToLowerInvariant()));

        public async Task<Embed> PollEmbed(string serverName, string hostName)
        {
            var info = await PollInfo(hostName);

            if (info.available)
            {
                EmbedBuilder builder = new EmbedBuilder().
                    WithTitle(string.IsNullOrEmpty(info.serverName) ? serverName : info.serverName)
                    .WithDescription($"Join at '{info.hostName}'!\n{info.playerCount} / {info.maxPlayers} players" + (info.ping > 0 ? $"at a {info.ping} millisecond ping." : ".")
                     + (string.IsNullOrEmpty(info.motd) ? "" : $"\n\nMessage of the Day:\n> {info.motd}"));

                var fields = new List<EmbedFieldBuilder>();

                if (!string.IsNullOrEmpty(info.gameName))
                {
                    fields.Add(new EmbedFieldBuilder().WithName("Game").WithValue(info.gameName).WithIsInline(true));
                }
                if (!string.IsNullOrEmpty(info.gameVersion))
                {
                    fields.Add(new EmbedFieldBuilder().WithName("Game Version").WithValue(info.gameVersion).WithIsInline(true));
                }
                if (!string.IsNullOrEmpty(info.gameMode))
                {
                    fields.Add(new EmbedFieldBuilder().WithName("Gamemode").WithValue(info.gameMode).WithIsInline(true));
                }
                if (info.tags != null && info.tags.Length > 0)
                {
                    fields.Add(new EmbedFieldBuilder().WithName("Server Tags").WithValue(string.Join(", ", info.tags)).WithIsInline(false));
                }
                /*if (!string.IsNullOrEmpty(info.hostAddress))
                {
                    fields.Add(new EmbedFieldBuilder().WithName("Server Address").WithValue(info.hostAddress).WithIsInline(false));
                }*/

                builder.WithFields(fields);
                builder.WithCurrentTimestamp();

                return builder.Build();
            }
            else
            {
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle(string.IsNullOrEmpty(info.serverName) ? serverName : info.serverName)
                    .WithDescription($"Currently unavailable at '{info.hostName}'.");
                return builder.Build();
            }
        }

        public async Task<string> PollString(string serverName, string hostName)
        {
            var info = await PollInfo(hostName);
            if (info.available)
            {
                return $"{(string.IsNullOrEmpty(info.serverName) ? serverName : info.serverName)}: '{info.hostName}' with {info.playerCount} / {info.maxPlayers} players at {info.ping} ping.";
            }
            else
            {
                return $"{serverName} is currently unreachable.";
            }
        }

        protected abstract Task<GenericGameServerInfo> PollInfo(string host);

        protected struct GenericGameServerInfo
        {
            public string hostAddress;
            public string hostName;
            public bool available;
            public string serverName;
            public string gameName;
            public string gameVersion;
            public string gameMode;
            public string motd;
            public string[] tags;
            public int playerCount;
            public int maxPlayers;
            public int ping;
        }
    }
}
