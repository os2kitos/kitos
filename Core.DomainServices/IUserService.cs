using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IUserService
    {
        User AddUser(User user);
        PasswordResetRequest IssuePasswordReset(User user);
        PasswordResetRequest GetPasswordReset(string requestId);
        void ResetPassword(string requestId, string newPassword);

        bool IsValidPassword(string password);
    }
}