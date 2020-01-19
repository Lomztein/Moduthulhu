using Lomztein.Moduthulhu.Core.IO.Database;
using Lomztein.Moduthulhu.Core.IO.Database.Factories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Karma
{
    public class SQLKarmaRepository
    {
        private const string _messagesTableName = "lomzkarma_messages";
        private const string _votesTableName = "lomzkarma_votes";

        private IDatabaseConnector GetConnector() => GenericFactory.SQL.Create() as IDatabaseConnector;

        public void Init ()
        {
            if (Database.GetDatabaseGroup () != "SQL")
            {
                throw new InvalidOperationException("Karma plugins custom repository currently only supports an SQL database.");
            }

            GetConnector().CreateTable(_messagesTableName, $"CREATE TABLE {_messagesTableName} (messageId text, channelId text)");
            GetConnector().CreateTable(_votesTableName, $"CREATE TABLE {_votesTableName} (messageId text, value int)");
        }
    }
}
