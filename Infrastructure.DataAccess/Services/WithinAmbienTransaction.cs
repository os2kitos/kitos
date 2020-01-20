using Infrastructure.Services.DataAccess;

namespace Infrastructure.DataAccess.Services
{
    public class WithinAmbienTransaction : IDatabaseTransaction
    {
        public void Dispose() { }

        public void Commit() { }

        public void Rollback() { }
    }
}
