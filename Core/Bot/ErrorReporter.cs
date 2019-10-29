using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.WebSocket;
using System.IO;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot
{
    internal class ErrorReporter
    {
        private Core ParentCore { get; set; }

        internal ErrorReporter (Core core) {
            ParentCore = core;
        }

        internal Task ReportError (Exception exception) {
            string message = (exception.Message + " - " + exception.StackTrace) + "\n";
            Cross.Log.Write (exception);
            File.AppendAllText (ParentCore.BaseDirectory + "errors.txt", message);
            return Task.CompletedTask;
        }
    }
}
