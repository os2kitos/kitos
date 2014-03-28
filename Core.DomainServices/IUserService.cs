using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IUserService
    {
        /* TODO: This doesn't really conform to single responsibility principle */
        User AddUser(User user);
        PasswordResetRequest IssuePasswordReset(User user);
        PasswordResetRequest GetPasswordReset(string requestId);
        void ResetPassword(PasswordResetRequest passwordResetRequest, string newPassword);
    }
}