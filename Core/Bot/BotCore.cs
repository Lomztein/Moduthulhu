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

        private BotClient _client;
        private ErrorReporter _errorReporter;

        internal static string BaseDirectory { get => AppContext.BaseDirectory; }
        internal static string DataDirectory { get => AppContext.BaseDirectory + "/Data"; }

        private CancellationTokenSource _shutdownToken = new CancellationTokenSource();

        internal async Task InitializeCore () {

            // Set up core
            BootDate = DateTime.Now;

            // Set up exception handler.
            _errorReporter = new ErrorReporter();

            // Set up client manager
            _client = new BotClient(this);
            _client.ExceptionCaught += OnExceptionCaught;
            _client.Initialize();

            Consent.Init();
            Localization.Init(new CultureInfo("en-US"));

            // Keep the core alive.
            try
            {
                await Task.Delay(-1, _shutdownToken.Token);
            } catch (TaskCanceledException exc)
            {
                Log.Write(Log.Type.BOT, $"Shutting down: {exc.Message}");
            }
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