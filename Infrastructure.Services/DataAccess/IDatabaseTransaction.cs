using System;

namespace Infrastructure.Services.DataAccess
{
    public interface IDatabaseTransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
