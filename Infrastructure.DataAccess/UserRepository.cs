using System.Linq;
using Core.DomainServices;
using Core.DomainModel;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries;
using Infrastructure.Services.Types;

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

        public User GetById(int id)
        {
            return _context.Users.SingleOrDefault(u => u.Id == id);
        }

        public IQueryable<User> Search(int id, Maybe<string> query)
        {
            return
                _context
                    .OrganizationRights
                    .ByOrganizationId(id)
                    .Select(x => x.User)
                    .Transform(userQuery =>
                        query.Select(queryString => new QueryUserByNameOrEmail(queryString).Apply(userQuery))
                            .GetValueOrFallback(userQuery))
                    .Distinct();
        }
    }
}