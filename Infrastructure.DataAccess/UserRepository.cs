using System;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using Core.DomainServices;
using Core.DomainModel;

namespace Infrastructure.DataAccess
{
    public class UserRepository : IUserRepository
    {
        private readonly KitosContext _context;
        private readonly DbSet<User> _users;
        private bool _disposed = false;

        public UserRepository(KitosContext context)
        {
            _context = context;
            _users = context.Users;
        }

        public User GetById(int id)
        {
            return _users.SingleOrDefault(u => u.Id == id);
        }

        public User GetByEmail(string email)
        {
            return _users.SingleOrDefault(u => u.Email == email);
        }

        public void Update(User user)
        {
            // TODO
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