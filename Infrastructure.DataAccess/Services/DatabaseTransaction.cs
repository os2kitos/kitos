using System;
using System.Data.Entity;
using Infrastructure.Services.DataAccess;

namespace Infrastructure.DataAccess.Services
{
    public class DatabaseTransaction : IDatabaseTransaction
    {
        private readonly DbContextTransaction _transaction;

        public DatabaseTransaction(DbContextTransaction transaction)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }
}
