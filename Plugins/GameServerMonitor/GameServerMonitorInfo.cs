using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.GameServerMonitor
{
    internal class GameServerMonitorInfo
    {
        public string GameName { get; set; }
        public string ServerName { get; set; }
        public string HostName { get; set; }  
        public ulong MessageChannelId { get; set; }
        public ulong MessageId { get; set; }
        public bool UseEmbed { get; set; }

        public GameServerMonitorInfo() { }

        public GameServerMonitorInfo(string gameName, string serverName, string hostName, ulong messageChannelId, ulong messageId, bool useEmbed)
        {
            GameName = gameName;
            ServerName = serverName;
            HostName = hostName;
            MessageChannelId = messageChannelId;
            MessageId = messageId;
            UseEmbed = useEmbed;
        }
    }
}
