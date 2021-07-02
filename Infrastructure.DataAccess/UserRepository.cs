using System.Linq;
using Core.DomainServices;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries.User;
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

        public IQueryable<User> GetGlobalAdmins()
        {
            return _context.Users.AsQueryable().Where(x => x.IsGlobalAdmin);
        }

        public User GetByEmail(string email)
        {
            return _context.Users.SingleOrDefault(u => u.Email == email);
        }

        public User GetById(int id)
        {
            return _context.Users.SingleOrDefault(u => u.Id == id);
        }

        public IQueryable<User> SearchOrganizationUsers(int organizationId, Maybe<string> query)
        {
            return
                _context
                    .OrganizationRights //The organization rights are what determines relationship to an organization
                    .ByOrganizationId(organizationId)
                    .Select(x => x.User)
                    .Transform(userQuery =>
                        query.Select(queryString => new QueryUserByNameOrEmail(queryString).Apply(userQuery))
                            .GetValueOrFallback(userQuery))
                    .Distinct();
        }

        public IQueryable<User> GetUsers()
        {
            return AsQueryable();
        }

        public IQueryable<User> GetUsersWithCrossOrganizationPermissions()
        {
            return AsQueryable().Transform(new QueryByCrossOrganizationPermissions().Apply);
        }

        public IQueryable<User> GetUsersWithRoleAssignment(OrganizationRole role)
        {
            return AsQueryable().Transform(new QueryByRoleAssignment(role).Apply);
        }

        public IQueryable<User> GetUsersInOrganization(int organizationId)
        {
            return AsQueryable().Where(user => user.OrganizationRights.Any(right => right.OrganizationId == organizationId));
        }
    }
}