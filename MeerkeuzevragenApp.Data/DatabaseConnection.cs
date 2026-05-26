using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DATA
{
    public class DatabaseConnection
    {
            public string ConnectionString { get; set; }
    
            public DatabaseConnection(string connectionString)
            {
                ConnectionString = connectionString;
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection("Server=localhost;Port=3306;Database=meerkeuzeDB;User ID=root;Password=root;");
        }
    }
}
