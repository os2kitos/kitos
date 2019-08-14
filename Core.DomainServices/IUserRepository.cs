using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IUserRepository : IGenericRepository<User>
    {
        User GetByEmail(string email);
        User GetByUuid(string uniqueId);
        User GetById(int id);
    }
}