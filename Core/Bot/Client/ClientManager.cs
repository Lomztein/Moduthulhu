using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class ClientManager
    {
        public Core Core { get; private set; }
        public BotClient[] ClientSlots { get; private set; } = new BotClient[0];
        public IEnumerable<SocketGuild> AllGuilds { get => ClientSlots.SelectMany (x => x.AllGuilds); }

        internal string ClientsDirectory { get => Core.BaseDirectory + "Clients"; }

        internal event Action<BotClient> ClientSpawned;
        internal event Action<BotClient> ClientKilled;
        internal event Func<Exception, Task> ExceptionCaught;

        internal ClientManager (Core core) {    
            Core = core;
        }

        internal BotClient SpawnClient (string name, int slotIndex) {
            BotClient client = new BotClient (this, name, slotIndex);
            client.InitializeShards ();
            ClientSlots[slotIndex] = client;
            ClientSpawned?.Invoke (client);
            client.ExceptionCaught += ExceptionCaught;
            return client;
        }

        internal async Task KillClient (BotClient client) {
            ClientSlots[client.ClientSlotIndex] = null;
            await client.Kill ();
            ClientKilled?.Invoke (client);
        }

        public async Task RestartClient (BotClient client) {
            string name = client.Name;
            int index = client.ClientSlotIndex;
            await KillClient (client);
            SpawnClient (name, index);
        }

        public bool IsClientAlive (int clientIndex) => ClientSlots[clientIndex] != null;

        private string[] GetClientPaths () {
            if (!Directory.Exists (ClientsDirectory))
                Directory.CreateDirectory (ClientsDirectory);
            return Directory.GetDirectories (ClientsDirectory); 
        }

        internal void InitializeClients () {

            string[] clients = GetClientPaths ();
            ClientSlots = new BotClient[clients.Length];
            for (int i = 0; i < clients.Length; i++) {

                string client = clients[i];
                string name = Path.GetFileName (client);
                BotClient newClient = SpawnClient (name, i);

            }

        }
    }
}
