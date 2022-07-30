using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace chitecapi
{
    class DataUtil
    {
        private string dbconection;
        private string connetionString;
        private SqlConnection sqlserverconection;
        private NpgsqlConnection postgresconection;
        private MySqlConnection mysqlconection;
        private string provider;
        private NpgsqlDataAdapter postgresdataAdapter;
        private SqlDataAdapter sqldataAdapter;
        private MySqlDataAdapter mysqldataAdapter;
        private SqlCommand sqlcommand;
        private NpgsqlCommand postgrescommand;
        private MySqlCommand mysqlcommand;



        public DataUtil()
        {
            dbconection= ConfigurationManager.AppSettings["activedbconnection"];
            connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;
            provider = ConfigurationManager.ConnectionStrings[dbconection].ProviderName;
        }

        public DataUtil(String conection)
        {
            dbconection = conection;
            connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;
            provider = ConfigurationManager.ConnectionStrings[dbconection].ProviderName;
        }

        public void Connect()
        {
            switch (provider)
            {
                case "Npgsql":
                    try
                    {
                        postgresconection = new NpgsqlConnection(connetionString);
                        postgresconection.Open();
                    }
                    catch (DbException e){
                        //MessageBox.Show(e.Message);
                    }
                    break;
                case "Mysql":
                    try
                    {
                        mysqlconection = new MySqlConnection(connetionString);
                        mysqlconection.Open();
                    }
                    catch (DbException e)
                    {
                        //MessageBox.Show(e.Message);
                    }
                    break;
                case "System.Data.SqlClient":
                    sqlserverconection = new SqlConnection(connetionString);
                    sqlserverconection.Open();
                    break;
                default:
                    sqlserverconection = new SqlConnection(connetionString);
                    sqlserverconection.Open();
                    break;
            }
          
        }

        public void FillDatatable(string sql,DataTable table)
        {
            switch (provider)
            {
                case "Npgsql":
                    try
                    {
                        postgresdataAdapter = new NpgsqlDataAdapter(sql, postgresconection);
                        postgresdataAdapter.Fill(table);
                    }
                    catch (DbException e)
                    {
                        //MessageBox.Show(e.Message);
                    }
                    break;
                case "Mysql":
                    try
                    {
                        mysqldataAdapter = new MySqlDataAdapter(sql, mysqlconection);
                        mysqldataAdapter.Fill(table);
                    }
                    catch (DbException e)
                    {
                        //MessageBox.Show(e.Message);
                    }
                    break;
                case "System.Data.SqlClient":
                    sqldataAdapter = new SqlDataAdapter(sql, sqlserverconection);
                    sqldataAdapter.Fill(table);
                    break;
                default:
                    sqldataAdapter = new SqlDataAdapter(sql, sqlserverconection);
                    sqldataAdapter.Fill(table);
                    break;
            }

        }

        public void FillDatatable(DataTable table)
        {
            switch (provider)
            {
                case "Npgsql":
                    try
                    {
                        postgresdataAdapter.Fill(table);
                    }
                    catch (DbException e)
                    {
                        //MessageBox.Show(e.Message);
                    }                    
                    break;
                case "Mysql":
                    try
                    {
                        mysqldataAdapter.Fill(table);
                    }
                    catch (DbException e)
                    {
                        //MessageBox.Show(e.Message);
                    }
                    break;
                case "System.Data.SqlClient":
                    sqldataAdapter.Fill(table);
                    break;
                default:
                    sqldataAdapter.Fill(table);
                    break;
            }

        }

        public void ExecuteCommand(string sql)
        {
            switch (provider)
            {
                case "Npgsql":
                    try
                    {
                        postgrescommand = new NpgsqlCommand(sql, postgresconection);
                        postgrescommand.CommandTimeout = 0;
                        postgrescommand.ExecuteNonQuery();
                    }
                    catch (DbException e)
                    {
                        //MessageBox.Show(e.Message);
                    }

                    break;
                case "Mysql":
                    try
                    {
                        mysqlcommand = new MySqlCommand(sql, mysqlconection);
                        mysqlcommand.CommandTimeout = 0;
                        mysqlcommand.ExecuteNonQuery();
                    }
                    catch (DbException e)
                    {
                        //MessageBox.Show(e.Message);
                    }

                    break;
                case "System.Data.SqlClient":
                    sqlcommand = new SqlCommand(sql, sqlserverconection);
                    sqlcommand.CommandTimeout = 0;
                    sqlcommand.ExecuteNonQuery();
                    break;
                default:
                    sqlcommand = new SqlCommand(sql, sqlserverconection);
                    sqlcommand.CommandTimeout = 0;
                    sqlcommand.ExecuteNonQuery();
                    break;
            }
        }

        public void PrepareStatement(string sql)
        {
            switch (provider)
            {
                case "Npgsql":
                    try
                    {
                        postgresdataAdapter = new NpgsqlDataAdapter(sql, postgresconection);
                    }
                    catch (DbException e)
                    {
                       // MessageBox.Show(e.Message);
                    }
                    break;
                case "Mysql":
                    try
                    {
                        mysqldataAdapter = new MySqlDataAdapter(sql, mysqlconection);
                    }
                    catch (DbException e)
                    {
                        // MessageBox.Show(e.Message);
                    }
                    break;
                case "System.Data.SqlClient":
                    sqldataAdapter = new SqlDataAdapter(sql, sqlserverconection);
                    break;
                default:
                    sqldataAdapter = new SqlDataAdapter(sql, sqlserverconection);
                    break;
            }
        }

        public void AddParameter(string key,Object value)
        {
            switch (provider)
            {
                case "Npgsql":
                    postgresdataAdapter.SelectCommand.Parameters.AddWithValue(key, value);
                    break;
                case "Mysql":
                    mysqldataAdapter.SelectCommand.Parameters.AddWithValue(key, value);
                    break;
                case "System.Data.SqlClient":
                    sqldataAdapter.SelectCommand.Parameters.AddWithValue(key, value);
                    break;
                default:
                    sqldataAdapter.SelectCommand.Parameters.AddWithValue(key, value);
                    break;
            }
        }

        public void ExecuteStatement()
        {
            switch (provider)
            {
                case "Npgsql":
                    try
                    {
                        postgresdataAdapter.SelectCommand.ExecuteNonQuery();
                    }
                    catch (DbException e)
                    {
                        //MessageBox.Show(e.Message);
                    }
                    break;
                case "Mysql":
                    try
                    {
                        mysqldataAdapter.SelectCommand.ExecuteNonQuery();
                    }
                    catch (DbException e)
                    {
                        //MessageBox.Show(e.Message);
                    }
                    break;
                case "System.Data.SqlClient":
                    sqldataAdapter.SelectCommand.ExecuteNonQuery();
                    break;
                default:
                    sqldataAdapter.SelectCommand.ExecuteNonQuery();
                    break;
            }
        }

        public void CloseConnection()
        {
            switch (provider)
            {
                case "Npgsql":
                    try
                    {
                        postgresconection.Close();
                        postgresconection.Dispose();
                    }
                    catch (DbException e)
                    {
                        //MessageBox.Show(e.Message);
                    }
                    break;
                case "Mysql":
                    try
                    {
                        mysqlconection.Close();
                        mysqlconection.Dispose();
                    }
                    catch (DbException e)
                    {
                        //MessageBox.Show(e.Message);
                    }
                    break;
                case "System.Data.SqlClient":
                    sqlserverconection.Close();
                    break;
                default:
                    sqlserverconection.Close();
                    break;
            }
            
        }

        public string GetLimit(int value)
        {
            string valor = "";

            switch (provider)
            {
                case "Npgsql":
                    valor = "limit "+value.ToString();
                    break;
                case "Mysql":
                    valor = "";
                    break;
                case "System.Data.SqlClient":
                    valor = "";
                    break;
                default:
                    valor = ""; 
                    break;
            }

            return valor;
        }


        public string GetTop(int value)
        {
            string valor = "";

            switch (provider)
            {
                case "Npgsql":
                    valor = "";
                    break;
                case "Mysql":
                    valor = "top " + value.ToString();
                    break;
                case "System.Data.SqlClient":
                    valor = "top " + value.ToString();
                    break;
                default:
                    valor = "top " + value.ToString();
                    break;
            }

            return valor;
        }


    }
}
