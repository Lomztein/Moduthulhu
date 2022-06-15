using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.GameServerMonitor.ServerMonitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.GameServerMonitor
{
    [Descriptor("Lomztein", "Game Server Monitor", "Monitor game servers using this nifty little tool where the bot will periodically update a message with information about the game server.")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Plugins/GameServerMonitor")]
    public class GameServerMonitorPlugin : PluginBase
    {
        private List<IGameServerMonitor> _monitors = new List<IGameServerMonitor>()
        {
            new MinecraftServerMonitor(),
        };
        internal CachedValue<List<GameServerMonitorInfo>> ServersToMonitor { get; private set; }
        private ServerMonitorCommandSet _commands;

        void AddGameServerMonitor(IGameServerMonitor monitor)
        {
            _monitors.Add(monitor);
        }

        public override void Initialize()
        {
            _commands = new ServerMonitorCommandSet() { ParentPlugin = this };

            GuildHandler.Clock.OnHourPassed += Clock_OnHourPassed;
            ServersToMonitor = GetDataCache("ServersToMonitor", x => new List<GameServerMonitorInfo>());
            RegisterMessageAction(nameof(AddGameServerMonitor), x => AddGameServerMonitor(x[0] as IGameServerMonitor));
            SendMessage("Moduthulhu-Command Root", "AddCommand", _commands);
        }

        internal bool AddServerToMonitor (GameServerMonitorInfo info)
        {
            var existing = ServersToMonitor.GetValue().Find(x => x.ServerName.ToLowerInvariant() == info.ServerName.ToLowerInvariant());
            if (existing == null)
            {
                ServersToMonitor.MutateValue(x => x.Add(info));
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool RemoveServerToMonitor(string serverName)
        {
            var info = GetServerMonitor(serverName);
            if (info != null)
            {
                ServersToMonitor.MutateValue(x => x.Remove(info));
                return true;
            }
            return false;
        }

        internal bool ServerMonitorExists(string serverName)
            => GetServerMonitor(serverName) != null;

        internal GameServerMonitorInfo GetServerMonitor(string serverName)
             => ServersToMonitor.GetValue().Find(x => x.ServerName.ToLowerInvariant() == serverName.ToLowerInvariant());

        internal bool HasMonitor(string gameName)
            => _monitors.Any(x => x.CanMonitorGame(gameName));

        internal async Task DeleteServerMonitoringMessage(GameServerMonitorInfo info)
        {
            SocketTextChannel channel = GuildHandler.GetTextChannel(info.MessageChannelId);
            var message = await channel.GetMessageAsync(info.MessageId);
            await message.DeleteAsync();
        }

        internal async Task<RestUserMessage> CreateServerMonitoringMessage(SocketTextChannel channel)
        {
            var message = await channel.SendMessageAsync("Fetching server info..");
            return message;
        }

        private async Task Clock_OnHourPassed(DateTime currentTick, DateTime lastTick)
        {
            await PollServers();
        }

        private IGameServerMonitor GetMonitor(string gameName)
            => _monitors.Find(x => x.CanMonitorGame(gameName));

        internal async Task PollServers() 
        { 
            foreach (GameServerMonitorInfo info in ServersToMonitor.GetValue())
            {
                await PollServer(info);
            }
        }

        internal async Task PollServer (GameServerMonitorInfo info)
        {
            var monitor = GetMonitor(info.GameName);
            SocketTextChannel channel = GuildHandler.GetTextChannel(info.MessageChannelId);
            if (channel != null)
            {
                IUserMessage message = await channel.GetMessageAsync(info.MessageId) as IUserMessage;
                if (message != null)
                {
                    if (info.UseEmbed)
                    {
                        Embed embed = await monitor.PollEmbed(info.ServerName, info.HostName);
                        await message.ModifyAsync(x => { x.Content = string.Empty; x.Embed = embed; });
                    }
                    else
                    {
                        string content = await monitor.PollString(info.ServerName, info.HostName);
                        await message.ModifyAsync(x => { x.Content = content; x.Embed = null; });
                    }
                }
            }
        }

        public override void Shutdown()
        {
            GuildHandler.Clock.OnHourPassed -= Clock_OnHourPassed;
            UnregisterMessageDelegate(nameof(AddGameServerMonitor));
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _commands);
        }
    }
}
