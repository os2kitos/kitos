using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IUserService
    {
        /* TODO: This doesn't really conform to single responsibility principle */
        User AddUser(User user, bool sendMailOnCreation, int orgId);
        void IssueAdvisMail(User user, bool reminder, int orgId);
        PasswordResetRequest IssuePasswordReset(User user, string subject, string content);
        PasswordResetRequest GetPasswordReset(string hash);
        void ResetPassword(PasswordResetRequest passwordResetRequest, string newPassword);

        User GetUserById(int id);
        //int GetCurrentOrganizationId(int userId);
    }
}