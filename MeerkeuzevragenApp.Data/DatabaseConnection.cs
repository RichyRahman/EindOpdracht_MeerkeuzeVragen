using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DATA
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;
            public DatabaseConnection()
            {
                _connectionString = ConfigurationManager.ConnectionStrings["MeerkeuzeDB"].ConnectionString;
            }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
