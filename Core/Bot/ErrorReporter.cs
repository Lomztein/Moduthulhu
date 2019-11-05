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
        internal Task ReportError (Exception exception) {
            string message = (exception.Message + " - " + exception.StackTrace) + "\n";
            Log.Write (exception);
            File.AppendAllText (Core.DataDirectory + "errors.txt", message);
            return Task.CompletedTask;
        }
    }
}
