using DataAccess;
using System;
using System.Data;

namespace Infrastructure.UnitOfWork
{
    public abstract class GenericTransactionalUnitOfWork : IDisposable
    {
        protected IDbConnection connection;
        protected IDbTransaction transaction;
        protected DataBaseAccess dataBaseAccess;

        protected GenericTransactionalUnitOfWork(IDbConnection connection)
        {
            this.connection = connection;
            connection.Open();
            transaction = connection.BeginTransaction();
            dataBaseAccess = new DataBaseAccess(transaction);
        }

        public void Commit()
        {
            try
            {
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction.Dispose();
                transaction = connection.BeginTransaction();
                ResetRepositories();
            }
        }

        protected virtual void ResetRepositories() { }

        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Dispose();
                transaction = null;
            }

            if (connection == null) return;

            connection.Dispose();
            connection = null;
        }
    }
}
