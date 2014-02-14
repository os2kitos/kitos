using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IPasswordResetRequestRepository
    {
        void Create(PasswordResetRequest passwordReset);

        PasswordResetRequest GetByHash(string hash);

        void Delete(PasswordResetRequest passwordReset);
    }
}