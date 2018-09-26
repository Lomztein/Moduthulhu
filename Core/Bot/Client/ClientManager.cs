using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    internal class ClientManager
    {
        internal Core Core { get; private set; }
        internal List<BotClient> ActiveClients { get; private set; } = new List<BotClient> ();
        public IEnumerable<SocketGuild> AllGuilds { get => ActiveClients.SelectMany (x => x.AllGuilds); }

        internal string ClientsDirectory { get => Core.BaseDirectory + "Clients"; }

        internal event Action<BotClient> OnClientSpawned;
        internal event Action<BotClient> OnClientKilled;
        internal event Action<Exception> OnExceptionCaught;

        internal ClientManager (Core core) {    
            Core = core;
        }

        internal BotClient SpawnClient (string name) {
            BotClient client = new BotClient (this, name);
            client.InitializeShards ();
            ActiveClients.Add (client);
            OnClientSpawned?.Invoke (client);
            client.OnExceptionCaught += OnExceptionCaught;
            return client;
        }

        internal async Task KillClient (BotClient client) {
            await client.Kill ();
            ActiveClients.Remove (client);
            OnClientKilled?.Invoke (client);
        }

        internal async Task RestartClient (BotClient client) {
            string name = client.Name;
            await KillClient (client);
            SpawnClient (name);
        }

        private string[] GetClientPaths () {
            if (!Directory.Exists (ClientsDirectory))
                Directory.CreateDirectory (ClientsDirectory);
            return Directory.GetDirectories (ClientsDirectory); 
        }

        internal void InitializeClients () {

            string[] clients = GetClientPaths ();
            foreach (string client in clients) {

                string name = Path.GetFileName (client);
                BotClient newClient = SpawnClient (name);

            }

        }
    }
}
