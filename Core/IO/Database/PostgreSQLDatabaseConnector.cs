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
        private string GetConnectionString() => Environment.GetEnvironmentVariable("PostgreSQLConnectionString");

        private NpgsqlConnection GetConnection (string connstring)
        {
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            return conn;
        }
        private async Task<NpgsqlConnection> GetConnectionAsync(string connstring)
        {
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            await conn.OpenAsync();
            return conn;
        }

        private NpgsqlCommand PrepareQuery (string query, Dictionary<string, object> parameters) 
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
                NpgsqlCommand cmd = PrepareQuery(query, parameters);
                cmd.Connection = connection;
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                {
                    DataSet data = new DataSet();
                    data.Reset();

                    adapter.Fill(data);
                    return ConvertDataSetToTableDictionary(data);
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

        private Dictionary<string, object>[] ConvertDataSetToTableDictionary (DataSet dataSet) 
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

        private Dictionary<string, object>[] Empty () => new Dictionary<string, object>[] { new Dictionary<string, object>() };
    }
}
