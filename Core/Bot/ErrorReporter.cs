using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.WebSocket;
using System.IO;

namespace Lomztein.Moduthulhu.Core.Bot
{

    internal class ErrorReporter
    {
        private Core ParentCore { get; set; }

        internal ErrorReporter (Core core) {
            ParentCore = core;
        }

        internal void ReportError (Exception exception) {

            string message = (exception.Message + " - " + exception.StackTrace);
            File.AppendAllText (ParentCore.BaseDirectory + "errors.txt", message);

        }
    }
}
