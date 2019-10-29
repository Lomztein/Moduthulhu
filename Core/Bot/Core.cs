using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Plugin;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Cross;
using Lomztein.Moduthulhu.Core.Bot.Client;

namespace Lomztein.Moduthulhu.Core.Bot {

    public class Core {
        public DateTime BootDate { get; private set; }
        public TimeSpan Uptime { get => DateTime.Now - BootDate; }

        private ClientManager _clientManager;
        private ErrorReporter _errorReporter;
        private UserList _coreAdministrators;

        internal string BaseDirectory { get => AppContext.BaseDirectory; }

        internal async Task InitializeCore () {

            // Set up core
            BootDate = DateTime.Now;
            _coreAdministrators = new UserList(Path.Combine(BaseDirectory, "AdministratorIDs"));
            Status.Set("CorePath", BaseDirectory);

            // Set up exception handler.
            _errorReporter = new ErrorReporter(this);

            // Set up client manager
            _clientManager = new ClientManager(this);
            _clientManager.InitializeClients();
            _clientManager.ExceptionCaught += OnExceptionCaught;

            // Keep the core alive.
            await Task.Delay (-1);
            Log.Write (Log.Type.BOT, "Shutting down..");
        }

        public bool IsCoreAdministrator (ulong userId)
        {
            return _coreAdministrators.Contains(userId);
        }

        private Task OnExceptionCaught(Exception exception)
        {
            return _errorReporter.ReportError(exception);
        }

        public string GetStatusString() => $"Core uptime: {Uptime}";
    }
}