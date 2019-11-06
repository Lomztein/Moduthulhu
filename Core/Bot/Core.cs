using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Plugin;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Core.Bot.Client;

namespace Lomztein.Moduthulhu.Core.Bot {

    public class Core {
        public DateTime BootDate { get; private set; }
        public TimeSpan Uptime { get => DateTime.Now - BootDate; }

        private BotClient _client;
        private ErrorReporter _errorReporter;

        internal static string BaseDirectory { get => AppContext.BaseDirectory; }
        internal static string DataDirectory { get => AppContext.BaseDirectory + "/Data"; }

        internal async Task InitializeCore () {

            // Set up core
            BootDate = DateTime.Now;

            // Set up exception handler.
            _errorReporter = new ErrorReporter();

            // Set up client manager
            _client = new BotClient(this);
            _client.ExceptionCaught += OnExceptionCaught;
            _client.Initialize();

            // Keep the core alive.
            await Task.Delay (-1);
            Log.Write (Log.Type.BOT, "Shutting down..");
        }

        private Task OnExceptionCaught(Exception exception)
        {
            return _errorReporter.ReportError(exception);
        }

        public string GetStatusString() => $"Core uptime: {Uptime}";
    }
}