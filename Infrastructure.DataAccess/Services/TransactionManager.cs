using System;
using System.Data;
using Infrastructure.Services.DataAccess;

namespace Infrastructure.DataAccess.Services
{
    public class TransactionManager : ITransactionManager, IDisposable
    {
        private readonly KitosContext _context;

        public TransactionManager(KitosContext context)
        {
            _context = context;
        }

        public IDatabaseTransaction Begin(IsolationLevel isolationLevel)
        {
            var currentTransaction = _context.Database.CurrentTransaction;
            if (currentTransaction != null)
            {
                return new WithinAmbienTransaction();
            }
            return new DatabaseTransaction(_context.Database.BeginTransaction(isolationLevel));
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
