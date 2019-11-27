using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Plugins;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Core.Bot.Client;
using System.Reflection;
using System.Globalization;

namespace Lomztein.Moduthulhu.Core.Bot {
    public class BotCore : IDisposable
    {
        public DateTime BootDate { get; private set; }
        public TimeSpan Uptime { get => DateTime.Now - BootDate; }

        public BotClient Client { get; private set; }
        private readonly ErrorReporter _errorReporter = new ErrorReporter ();

        internal static string BaseDirectory { get => AppContext.BaseDirectory; }
        internal static string DataDirectory { get => AppContext.BaseDirectory + "/Data"; }

        private readonly CancellationTokenSource _shutdownToken = new CancellationTokenSource();

        internal async Task InitializeCore(string[] args)
        {

            // Set up core
            BootDate = DateTime.Now;

            // Set up client manager
            Client = new BotClient(this);
            Client.ExceptionCaught += OnExceptionCaught;
            Client.Initialize().GetAwaiter().GetResult();

            Consent.Init();
            Localization.Init(new CultureInfo("en-US"));

            // Keep the core alive.
            await Task.Delay(-1, _shutdownToken.Token);
            Log.Write(Log.Type.BOT, $"Shutting down..");
            Environment.Exit(0);
        }

        public void Shutdown ()
        {
            _shutdownToken.Cancel();
        }

        private Task OnExceptionCaught(Exception exception)
        {
            return _errorReporter.ReportError(exception);
        }

        public string GetStatusString() => $"Core uptime: {Uptime}";

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool managed)
        {
            if (managed)
            {
                _shutdownToken.Dispose();
            }
        }
    }
}