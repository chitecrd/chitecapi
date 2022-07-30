using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IGenericRepository<T>
    {
        Task<IEnumerable<T>> LoadAsync(string sql, dynamic parameters);
        Task<T> LoadFirstAsync(string sql, dynamic parameters);
        Task SaveAsync(string sql, dynamic parameters);
        Task<TU> SaveAndReturnAsync<TU>(string sql, dynamic parameters);
    }
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DataBaseAccess dataBaseAccess;

        protected GenericRepository(DataBaseAccess dataBaseAccess)
        {
            this.dataBaseAccess = dataBaseAccess;
        }

        public virtual async Task<IEnumerable<T>> LoadAsync(string sql, dynamic parameters)
        {
            return await dataBaseAccess.LoadDataAsync<T, dynamic>(sql, parameters);
        }

        public virtual async Task<T> LoadFirstAsync(string sql, dynamic parameters)
        {
            return await dataBaseAccess.LoadFirstOrDefaultAsync<T, dynamic>(sql, parameters);
        }

        public virtual async Task SaveAsync(string sql, dynamic parameters)
        {
            await dataBaseAccess.SaveDataAsync(sql, parameters);
        }

        public virtual async Task<TU> SaveAndReturnAsync<TU>(string sql, dynamic parameters)
        {
            return await dataBaseAccess.ExecuteScalarAsync<TU, dynamic>(sql, parameters);
        }
    }
}
