using System.Linq;
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

        public User GetByUuid(string uniqueId)
        {
            return _context.Users.SingleOrDefault(u => u.UniqueId == uniqueId);
        }

        public User GetById(int id)
        {
            return _context.Users.SingleOrDefault(u => u.Id == id);
        }
    }
}