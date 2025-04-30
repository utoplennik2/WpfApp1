using MySql.Data.MySqlClient;

namespace HospitalConsoleSelect
{
    public static class DBMySQLUtils
    {
        public static MySqlConnection GetDBConnection(string host, int port, string database, string username, string password)
        {
            string connString = $"Server={host};Port={port};Database={database};Uid={username};Pwd={password};";
            MySqlConnection conn = new MySqlConnection(connString);
            return conn;
        }
    }
}