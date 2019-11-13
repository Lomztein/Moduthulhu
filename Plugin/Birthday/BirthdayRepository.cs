using Lomztein.Moduthulhu.Core.IO.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Birthday
{
    public class BirthdayRepository
    {
        private IDatabaseConnector GetConnector() => new PostgreSQLDatabaseConnector();
        private string _table;

        public BirthdayRepository (string table)
        {
            _table = table;
            GetConnector().CreateTable(table, $"CREATE TABLE {_table} (guildid text, userid text, birthdate date, lastcelebratedyear integer)");
        }

        public Dictionary<string, object>[] GetBirthdates(ulong guildId) => GetConnector().ReadQuery($"SELECT userid, birthdate, lastcelebratedyear FROM {_table} WHERE guildid = @guildid", new Dictionary<string, object>() { { "@guildid", guildId.ToString() } });

        public void UpdateBirthdate(ulong guildId, ulong userId, DateTime date, int lastCelebratedYear) => GetConnector().UpdateQuery($"UPDATE {_table} SET birthdate = @date, @lastcelebratedyear = @lastcelebratedyear WHERE guildid = @guildid AND userid = @userid",
            new Dictionary<string, object>() { { "@date", date }, { "@lastcelebratedyear", lastCelebratedYear }, { "@guildid", guildId.ToString() }, { "@userid", userId.ToString() } });

        public void InsertBirthdate(ulong guildId, ulong userId, DateTime date, int lastCelebratedYear) =>
            GetConnector().UpdateQuery($"INSERT INTO {_table} VALUES (@guildid, @userid, @date, @lastcelebratedyear)",
                new Dictionary<string, object>() { { "@guildid", guildId.ToString() }, { "@userid", userId.ToString() }, { "@date", date }, { "@lastcelebratedyear", lastCelebratedYear } });

    }
}
