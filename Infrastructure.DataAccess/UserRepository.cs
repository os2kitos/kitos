using System;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Core.DomainServices;
using Core.DomainModel;

namespace Infrastructure.DataAccess
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly KitosContext _context;

        public UserRepository(KitosContext context)
            : base(context)
        {
            _context = context;
        }

        public User GetByEmail(string email)
        {
            return _context.Users.SingleOrDefault(u => u.Email == email);
        }

    }
}