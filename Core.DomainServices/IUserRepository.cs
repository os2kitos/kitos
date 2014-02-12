using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IUserRepository
    {
        User GetById(int id);
        User GetByUsername(string username);
        User GetByEmail(string email);

        void Update(User user);

        //TODO: Should this even be here - is it really the repository's responsibility??
        bool Validate(string username, string password);
    }
}