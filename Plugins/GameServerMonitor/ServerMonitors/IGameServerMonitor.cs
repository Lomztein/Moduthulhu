using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.GameServerMonitor.ServerMonitors
{
    public interface IGameServerMonitor
    {
        bool CanMonitorGame(string gameName);

        Task<Embed> PollEmbed(string serverName, string hostName);

        Task<string> PollString(string serverName, string hostName);
    }
}
