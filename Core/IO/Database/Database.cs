using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database
{
    public static class Database
    {
        private const string DatabaseTypeEnvVariableName = "DATABASE_TYPE";
        private const char GroupTypeSeperator = '/';
        private const string DefaultType = "Path/Json";

        public static string GetDatabaseTypeString() {
            string type = Environment.GetEnvironmentVariable(DatabaseTypeEnvVariableName);
            if (type == null)
            {
                type = DefaultType;
            }
            return type;
        }

        public static string GetDatabaseGroup(string typeString)
            => typeString.Split(GroupTypeSeperator)[0];

        public static string GetDatabaseGroup() => GetDatabaseGroup(GetDatabaseTypeString ());

        public static string GetDatabaseType(string typeString)
        {
            string[] split = typeString.Split(GroupTypeSeperator);
            return split.Length == 2 ? split[1] : split[0];
        }

        public static string GetDatabaseType() => GetDatabaseType(GetDatabaseTypeString());
    }
}
