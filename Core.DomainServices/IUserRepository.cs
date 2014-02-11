using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IUserRepository
    {
        User Get(int id);
        User Get(string username);
        User Get(string username, string password);
        bool Validate(string username, string password);
    }
}