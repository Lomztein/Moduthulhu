using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Module;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Cross;
using Lomztein.Moduthulhu.Core.Bot.Client;

namespace Lomztein.Moduthulhu.Core.Bot {

    /// <summary>
    /// A wrapper for the Discord.NET DiscordClient.
    /// </summary>
    public class Core {

        // TODO: Split BotClient into a Core class and a IClientWrapper interface.
        // TODO: Move the Clock systems into the main Core area.
        // TODO: Allow for on-the-fly changing of avatar/username
        // TODO: Allow for sending bot-wide messages directly in console.

        public DateTime BootDate { get; private set; }

        public ClientManager ClientManager { get; private set; }
        public ModuleLoader ModuleLoader { get; private set; }
        public Clock.Clock Clock { get; private set; }

        private string Token { get => File.ReadAllLines (BaseDirectory + "/token.txt")[0]; }

        public string BaseDirectory { get => AppContext.BaseDirectory; }

        private DiscordSocketConfig socketConfig = new DiscordSocketConfig () {
            DefaultRetryMode = RetryMode.AlwaysRetry,
        };

        public async Task InitializeCore() {
            BootDate = DateTime.Now;

            ClientManager = new ClientManager (this);
            ModuleLoader = new ModuleLoader (this);
            Clock = new Clock.Clock (1);

            await Task.Delay (-1);
            Log.Write (Log.Type.BOT, "Shutting down..");
        }

        private Task OnLog(LogMessage log) {
            Log.Write (Log.Type.BOT, log.Severity + " - " + log.Message);
            if (log.Exception != null)
                Log.Write (log.Exception);
            return Task.CompletedTask;
        }

    }
}