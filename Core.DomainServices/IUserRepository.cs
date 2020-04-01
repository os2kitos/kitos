using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IUserRepository : IGenericRepository<User>
    {
        User GetByEmail(string email);
        User GetById(int id);
    }
}