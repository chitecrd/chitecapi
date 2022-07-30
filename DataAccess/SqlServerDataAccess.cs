using Dapper;
using MySql.Data.MySqlClient;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class SqlServerDataAccess
    {
        public SqlServerDataAccess(string connectionString)
        {
            ConnectionString = connectionString;
            Provider = "System.Data.SqlClient";
        }


        public SqlServerDataAccess(string connectionString,string provider)
        {
            ConnectionString = connectionString;
            Provider = provider;
        }

        public string ConnectionString { get; set; }
        public string Provider { get; set; }

        public async Task<bool> ValidateAsync<T>(
            string sql, T parameters, CommandType? commandType = null)
        {
            IDbConnection connectionval;
            switch (Provider)
            {
                case "Npgsql":                    
                    connectionval = new NpgsqlConnection(ConnectionString);          
                    break;
                case "Mysql":
                    connectionval = new MySqlConnection(ConnectionString);
                    break;
                case "System.Data.SqlClient":
                    connectionval = new SqlConnection(ConnectionString);
                    break;
                default:
                    connectionval = new SqlConnection(ConnectionString);
                    break;
            }
            

            using (IDbConnection connection = connectionval)
            {
                return await connection.ExecuteScalarAsync<bool>(sql, parameters, commandType: commandType);
            }
        }

        public async Task<T> LoadFirstOrDefaultAsync<T, TU>(
            string sql, TU parameters, CommandType? commandType = null)
        {
            IDbConnection connectionval;
            switch (Provider)
            {
                case "Npgsql":
                    connectionval = new NpgsqlConnection(ConnectionString);
                    break;
                case "Mysql":
                    connectionval = new MySqlConnection(ConnectionString);
                    break;
                case "System.Data.SqlClient":
                    connectionval = new SqlConnection(ConnectionString);
                    break;
                default:
                    connectionval = new SqlConnection(ConnectionString);
                    break;
            }
            using (IDbConnection connection = connectionval)
            {
                return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, commandType: commandType);
            }
        }

        public async Task<List<T>> LoadDataAsync<T, TU>(
            string sql, TU parameters, CommandType? commandType = null)
        {
            IDbConnection connectionval;
            switch (Provider)
            {
                case "Npgsql":
                    connectionval = new NpgsqlConnection(ConnectionString);
                    break;
                case "Mysql":
                    connectionval = new MySqlConnection(ConnectionString);
                    break;
                case "System.Data.SqlClient":
                    connectionval = new SqlConnection(ConnectionString);
                    break;
                default:
                    connectionval = new SqlConnection(ConnectionString);
                    break;
            }
            using (IDbConnection connection = connectionval)
            {
                var data = await connection.QueryAsync<T>(sql, parameters, commandType: commandType);

                return data.ToList();
            }
        }

        public List<T> LoadData<T, TU>(
            string sql, TU parameters, CommandType? commandType = null)
        {
            IDbConnection connectionval;
            switch (Provider)
            {
                case "Npgsql":
                    connectionval = new NpgsqlConnection(ConnectionString);
                    break;
                case "Mysql":
                    connectionval = new MySqlConnection(ConnectionString);
                    break;
                case "System.Data.SqlClient":
                    connectionval = new SqlConnection(ConnectionString);
                    break;
                default:
                    connectionval = new SqlConnection(ConnectionString);
                    break;
            }
            using (IDbConnection connection = connectionval)
            {
                var data = connection.Query<T>(sql, parameters, commandType: commandType);

                return data.ToList();
            }
        }

        public async Task SaveDataAsync<T>(
            string sql, T parameters, CommandType? commandType = null)
        {
            IDbConnection connectionval;
            switch (Provider)
            {
                case "Npgsql":
                    connectionval = new NpgsqlConnection(ConnectionString);
                    break;
                case "Mysql":
                    connectionval = new MySqlConnection(ConnectionString);
                    break;
                case "System.Data.SqlClient":
                    connectionval = new SqlConnection(ConnectionString);
                    break;
                default:
                    connectionval = new SqlConnection(ConnectionString);
                    break;
            }
            using (IDbConnection connection = connectionval)
            {
                await connection.ExecuteAsync(sql, parameters, commandType: commandType);
            }
        }

        public async Task SaveDataAsync<T>(
           string sql, T parameters, string databasename, CommandType? commandType = null)
        {
            IDbConnection connectionval;
            switch (Provider)
            {
                case "Npgsql":
                    connectionval = new NpgsqlConnection(ConnectionString);
                    break;
                case "Mysql":
                    connectionval = new MySqlConnection(ConnectionString);
                    break;
                case "System.Data.SqlClient":
                    connectionval = new SqlConnection(ConnectionString);
                    break;
                default:
                    connectionval = new SqlConnection(ConnectionString);
                    break;
            }
            using (IDbConnection connection = connectionval)
            {
                string actualdatabase = connection.Database;
                connection.Open();
                connection.ChangeDatabase(databasename);
                await connection.ExecuteAsync(sql, parameters, commandType: commandType);
                connection.ChangeDatabase(actualdatabase);
                connection.Close();
            }
        }

        public void SaveData<T>(
            string sql, T parameters, CommandType? commandType = null)
        {
            IDbConnection connectionval;
            switch (Provider)
            {
                case "Npgsql":
                    connectionval = new NpgsqlConnection(ConnectionString);
                    break;
                case "Mysql":
                    connectionval = new MySqlConnection(ConnectionString);
                    break;
                case "System.Data.SqlClient":
                    connectionval = new SqlConnection(ConnectionString);
                    break;
                default:
                    connectionval = new SqlConnection(ConnectionString);
                    break;
            }
            using (IDbConnection connection = connectionval)
            {
                connection.Execute(sql, parameters, commandType: commandType);
            }
        }
    }
}
