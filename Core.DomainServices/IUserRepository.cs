using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IUserRepository
    {
        User GetById(int id);
        User GetByEmail(string email);

        void Update(User user);
    }
}