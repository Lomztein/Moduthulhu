using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    internal class ClientManager
    {
        internal Core Core { get; private set; }
        internal List<BotClient> ActiveClients { get; private set; }

        internal string ClientsDirectory { get => Core.BaseDirectory + "//Clients"; }

        internal ClientManager (Core core) {
            Core = core;
        }

        internal BotClient SpawnClient (string token, string name) {
            BotClient client = new BotClient (this, token, name);
            ActiveClients.Add (client);
            return client;
        }

        internal void KillClient (BotClient client) {
            client.Kill ();
            ActiveClients.Remove (client);
        }

        private string[] GetClientPaths () {
            if (!Directory.Exists (ClientsDirectory))
                Directory.CreateDirectory (ClientsDirectory);
            return Directory.GetDirectories (ClientsDirectory); 
        }
    }
}
