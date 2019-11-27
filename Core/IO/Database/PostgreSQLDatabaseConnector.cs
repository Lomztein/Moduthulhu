using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;
using Npgsql;
using System.Data;

namespace Lomztein.Moduthulhu.Core.IO.Database
{
    public class PostgreSQLDatabaseConnector : IDatabaseConnector
    {
        private static string GetConnectionString() => Environment.GetEnvironmentVariable("PostgreSQLConnectionString");

        private static NpgsqlConnection GetConnection (string connstring)
        {
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            return conn;
        }

        private static NpgsqlCommand PrepareQuery (string query, Dictionary<string, object> parameters) 
        {
            NpgsqlCommand cmd = new NpgsqlCommand(query);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }
            return cmd;
        }

        public Dictionary<string, object>[] ReadQuery(string query, Dictionary<string, object> parameters)
        {
            using (NpgsqlConnection connection = GetConnection(GetConnectionString()))
            {
                using (NpgsqlCommand cmd = PrepareQuery(query, parameters))
                {
                    cmd.Connection = connection;
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                    {
                        using (DataSet data = new DataSet())
                        {
                            data.Reset();

                            adapter.Fill(data);
                            return ConvertDataSetToTableDictionary(data);
                        }
                    }
                }
            }
        }

        public void UpdateQuery (string query, Dictionary<string, object> parameters)
        {
            using (NpgsqlConnection connection = GetConnection(GetConnectionString()))
            {
                using (NpgsqlCommand cmd = PrepareQuery(query, parameters))
                {
                    cmd.Connection = connection;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static Dictionary<string, object>[] ConvertDataSetToTableDictionary (DataSet dataSet) 
        {
            if (dataSet.Tables.Count > 1) 
            {
                throw new InvalidOperationException("You cannot convert a multitable dataset into a single table dictionary.");
            }

            DataTable table = dataSet.Tables[0];

            int rowCount = table.Rows.Count;
            int columnCount = table.Columns.Count;

            var tableDict = new Dictionary<string, object>[rowCount];

            for (int i = 0; i < rowCount; i++)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                var row = table.Rows[i];

                for (int j = 0; j < columnCount; j++)
                {
                    string columnName = table.Columns[j].ColumnName.ToLower ();
                    dict.Add(columnName, row.ItemArray[j]);
                }

                tableDict[i] = dict;
            }

            return tableDict;
        }

        public void CreateTable(string tableName, string createQuery)
        {
            var res = ReadQuery("SELECT 1 FROM information_schema.tables WHERE table_name = @table", new Dictionary<string, object> { { "@table", tableName } });

            try
            {
                if (res.Length == 0)
                {
                    Log.Write(Log.Type.DATA, $"Creating missing database table {tableName}.");
                    UpdateQuery(createQuery, new Dictionary<string, object>());
                }
            }
            catch (NpgsqlException e)
            {
                Log.Exception(e);
            }
        }
    }
}
