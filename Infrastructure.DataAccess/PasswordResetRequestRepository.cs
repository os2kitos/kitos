using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Infrastructure.DataAccess
{
    public class PasswordResetRequestRepository : IPasswordResetRequestRepository
    {
        private readonly KitosContext _context;
        private readonly DbSet<PasswordResetRequest> _resets;
        private bool _disposed = false;

        public PasswordResetRequestRepository(KitosContext context)
        {
            _context = context;
            _resets = context.PasswordResetRequests;
        }

        public void Create(PasswordResetRequest passwordReset)
        {
            //TODO
            throw new NotImplementedException();
        }

        public PasswordResetRequest GetByHash(string hash)
        {
            return _resets.SingleOrDefault(r => r.Id == hash);
        }

        public void Delete(PasswordResetRequest passwordReset)
        {
            //TODO
            throw new NotImplementedException();
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}