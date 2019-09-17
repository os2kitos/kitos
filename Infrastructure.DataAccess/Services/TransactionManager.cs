using System.Data;
using Infrastructure.Services.DataAccess;

namespace Infrastructure.DataAccess.Services
{
    public class TransactionManager : ITransactionManager
    {
        private readonly KitosContext _context;

        public TransactionManager(KitosContext context)
        {
            _context = context;
        }

        public IDatabaseTransaction Begin(IsolationLevel isolationLevel)
        {
            return new DatabaseTransaction(_context.Database.BeginTransaction(isolationLevel));
        }
    }
}
