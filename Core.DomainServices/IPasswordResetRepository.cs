using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IPasswordResetRepository
    {
        void Create(PasswordReset passwordReset);

        PasswordReset Get(string hash);

        void Delete(PasswordReset passwordReset);
    }
}