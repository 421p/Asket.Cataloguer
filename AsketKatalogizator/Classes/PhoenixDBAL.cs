using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FirebirdSql.Data.FirebirdClient;

namespace AsketKatalogizator
{              //DataBase Abstraction Level
    class PhoenixDBAL : IDisposable {
        private readonly FbConnection _conn;

        public PhoenixDBAL(string connectionString) {
            _conn = new FbConnection(connectionString);
            _conn.Open();
            
        }

        /// <summary>
        /// Returns a list of resulted rows represented as arrays of objects.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<object[]> FetchAll(string query) {

            var returnable = new List<object[]>();

            var reader = new FbCommand(query, _conn).ExecuteReader();

            while (reader.Read()) {
                int limit = reader.FieldCount;
                var row = new object[limit];
                for (int i = 0; i < limit; i++) {
                    row[i] = reader.GetValue(i);
                }
                returnable.Add(row);
            }

            return returnable;
        }

        /// <summary>
        /// Executes a statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="query">string</param>
        /// <returns>int</returns>
        public int ExecuteUpdate(string query) {
            try {
                return new FbCommand(query, _conn).ExecuteNonQuery();
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                return 0;
            }
        }


        public void Dispose() {
            _conn.Close();
            _conn.Dispose();
        }
    }
}
