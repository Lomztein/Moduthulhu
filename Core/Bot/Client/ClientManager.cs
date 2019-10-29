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
        private BotClient[] _clients;

        internal string ClientsDirectory { get => Core.BaseDirectory + "Clients"; }

        internal event Func<Exception, Task> ExceptionCaught;

        internal ClientManager (Core core) {    
            Core = core;
        }

        public bool IsClientAlive (int clientIndex) => _clients[clientIndex] != null;

        private string[] GetClientPaths () {
            if (!Directory.Exists (ClientsDirectory))
                Directory.CreateDirectory (ClientsDirectory);
            return Directory.GetDirectories (ClientsDirectory); 
        }

        private Task OnExceptionCaught(Exception exception)
        {
            return ExceptionCaught?.Invoke(exception);
        }

        internal void InitializeClients () {

            string[] clientPaths = GetClientPaths ();
            _clients = new BotClient[clientPaths.Length];

            for (int i = 0; i < _clients.Length; i++) {
                string path = clientPaths[i];
                string name = Path.GetFileName (path);

                BotClient client = new BotClient(this, name);
                _clients[i] = client;
                client.ExceptionCaught += OnExceptionCaught;
            }
        }
    }
}
