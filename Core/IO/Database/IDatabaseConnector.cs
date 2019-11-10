using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.IO.Database
{
    public interface IDatabaseConnector
    {
        Dictionary<string, object>[] ReadQuery(string queryString, Dictionary<string, object> parameters);

        void UpdateQuery(string queryString, Dictionary<string, object> parameters);

        void CreateTable(string tableName, string createQuery);
    }
}
