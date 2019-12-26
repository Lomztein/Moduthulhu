using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.WebSocket;
using System.IO;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.IO.Database;
using Lomztein.Moduthulhu.Core.IO.Database.Factories;

namespace Lomztein.Moduthulhu.Core.Bot
{
    internal class ErrorReporter
    {
        private static IDatabaseConnector GetConnector() => GenericFactory.SQL.Create() as IDatabaseConnector;

        private static bool UsesSQLDatabase() => Database.GetDatabaseType() == "SQL";

        internal ErrorReporter ()
        {
            if (UsesSQLDatabase ())
            {
                GetConnector().CreateTable("errors", "CREATE TABLE errors (type text, date timestamp, target text, message text, stacktrace text)");
            }
        }

        private static Task ReportError (Exception exception) {
            Log.Exception (exception);
            if (UsesSQLDatabase ())
            {
                GetConnector().UpdateQuery("INSERT INTO errors VALUES (@type, @date, @target, @message, @stacktrace)", new Dictionary<string, object> { { "@type", exception.GetType().Name }, { "@date", DateTime.Now }, { "@target", exception.TargetSite.ToString() }, { "@message", exception.Message }, { "@stacktrace", exception.StackTrace } });
            }
            if (exception.InnerException != null)
            {
                ReportError(exception.InnerException);
            }
            return Task.CompletedTask;
        }
    }
}
