using System.Linq;
using Core.DomainModel;
using Infrastructure.Services.Types;

namespace Core.DomainServices
{
    public interface IUserRepository : IGenericRepository<User>
    {
        IQueryable<User> GetGlobalAdmins();
        User GetByEmail(string email);
        User GetById(int id);
        IQueryable<User> SearchOrganizationUsers(int organizationId, Maybe<string> query);
        IQueryable<User> GetUsers();
    }
}