using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IPasswordResetRequestRepository
    {
        void Create(PasswordResetRequest passwordReset);

        PasswordResetRequest Get(string hash);

        void Delete(PasswordResetRequest passwordReset);
    }
}