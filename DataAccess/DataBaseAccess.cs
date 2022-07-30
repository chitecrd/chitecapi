using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class DataBaseAccess
    {
        private readonly IDbTransaction transaction;
        private readonly IDbConnection connection;

        public DataBaseAccess(IDbTransaction transaction)
        {
            this.transaction = transaction;
            connection = transaction.Connection;
        }

        public async Task<bool> ValidateAsync<T>(
            string sql, T parameters, CommandType? commandType = null)
        {
            return await connection
                .ExecuteScalarAsync<bool>(sql, parameters, transaction, commandType: commandType);
        }

        public async Task<T> LoadFirstOrDefaultAsync<T, TU>(
            string sql, TU parameters, CommandType? commandType = null)
        {
            return await connection
                .QueryFirstOrDefaultAsync<T>(sql, parameters, transaction, commandType: commandType);
        }

        public async Task<List<T>> LoadDataAsync<T, TU>(
            string sql, TU parameters, CommandType? commandType = null)
        {
            var data = await connection
                .QueryAsync<T>(sql, parameters, transaction, commandType: commandType);

            return data.ToList();
        }

        public List<T> LoadData<T, TU>(
            string sql, TU parameters, CommandType? commandType = null)
        {
            var data = connection
                .Query<T>(sql, parameters, transaction, commandType: commandType);

            return data.ToList();
        }

        public async Task SaveDataAsync<T>(
            string sql, T parameters, CommandType? commandType = null)
        {
            await connection
                .ExecuteAsync(sql, parameters, transaction, commandType: commandType);
        }

        public async Task<T> ExecuteScalarAsync<T, TU>(
            string sql, TU parameters, CommandType? commandType = null)
        {
            return await connection
                .ExecuteScalarAsync<T>(sql, parameters, transaction, commandType: commandType);
        }

        public void SaveData<T>(
            string sql, T parameters, CommandType? commandType = null)
        {
            connection
                .Execute(sql, parameters, transaction, commandType: commandType);
        }
    }
}
