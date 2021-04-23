using System;
using Infrastructure.Services.DataAccess;

namespace Infrastructure.DataAccess.Services
{
    public class EntityFrameworkContextDatabaseControl : IDatabaseControl, IDisposable
    {
        private readonly KitosContext _kitosContext;

        public EntityFrameworkContextDatabaseControl(KitosContext kitosContext)
        {
            _kitosContext = kitosContext;
        }

        public void Dispose()
        {
            _kitosContext?.Dispose();
        }

        public void SaveChanges()
        {
            _kitosContext.SaveChanges();
        }
    }
}
