using Lomztein.Moduthulhu.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.GameServerMonitor.ServerMonitors
{
    public class MinecraftServerMonitor : GameServerMonitorBase
    {
        protected override string[] SupportedGames => new [] { "Minecraft" };

        protected override async Task<GenericGameServerInfo> PollInfo(string host)
        {
            var apiHost = $"https://api.mcsrvstat.us/2/{host}";
            var info = new GenericGameServerInfo();

            try
            {
                JObject obj = await HTTP.GetJSON(new Uri(apiHost));

                info.available = true;
                info.hostAddress = $"{obj.GetValue("ip")}:{obj.GetValue("port")}";
                info.hostName = obj.GetValue("hostname").ToString();

                if (obj.TryGetValue("motd", out JToken motd))
                {
                    if (motd is JObject motdObj)
                    {
                        info.motd = (motdObj.GetValue("clean") as JArray)[0].ToString();
                    }
                    else
                    {
                        info.motd = motd.ToString();
                    }
                }
                if (obj.TryGetValue("players", out JToken players))
                {
                    var playersObj = players as JObject;
                    info.playerCount = playersObj.GetValue("online").ToObject<int>();
                    info.maxPlayers = playersObj.GetValue("max").ToObject<int>();
                }
                if (obj.TryGetValue("version", out JToken version))
                {
                    info.gameVersion = version.ToString();
                }

            } catch (Exception)
            {
                info.available = false;
            }

            return info;
        }
    }
}
