using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.WebSocket;
using System.IO;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.IO.Database;

namespace Lomztein.Moduthulhu.Core.Bot
{
    internal class ErrorReporter
    {
        private IDatabaseConnector GetConnector () => new PostgreSQLDatabaseConnector();

        internal ErrorReporter ()
        {
            GetConnector().CreateTable("errors", "CREATE TABLE errors (type text, date timestamp, target text, message text, stacktrace text)");
        }

        internal Task ReportError (Exception exception) {
            string message = (exception.Message + " - " + exception.StackTrace) + "\n";
            Log.Write (exception);
            GetConnector().UpdateQuery("INSERT INTO errors VALUES (@type, @date, @target, @message, @stacktrace)", new Dictionary<string, object>() { { "@type", exception.GetType().Name }, { "@date", DateTime.Now }, { "@target", exception.TargetSite.ToString () }, { "@message", exception.Message }, { "@stacktrace", exception.StackTrace } });
            return Task.CompletedTask;
        }
    }
}
