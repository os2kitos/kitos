using System.Data;

namespace Infrastructure.Services.DataAccess
{
    public interface ITransactionManager
    {
        IDatabaseTransaction Begin(IsolationLevel isolationLevel);
    }
}
