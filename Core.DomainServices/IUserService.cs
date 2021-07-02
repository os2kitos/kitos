using System;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;

namespace Core.DomainServices
{
    public interface IUserService : IDisposable
    {
        User AddUser(User user, bool sendMailOnCreation, int orgId);
        void IssueAdvisMail(User user, bool reminder, int orgId);
        PasswordResetRequest IssuePasswordReset(User user, string subject, string content);
        PasswordResetRequest GetPasswordReset(string hash);
        void ResetPassword(PasswordResetRequest passwordResetRequest, string newPassword);
        Result<IQueryable<User>, OperationError> GetUsersWithCrossOrganizationPermissions();
        Result<IQueryable<User>, OperationError> GetUsersWithRoleAssignedInAnyOrganization(OrganizationRole role);
        Result<IQueryable<User>, OperationError> GetUsersInOrganization(Guid organizationUuid);
        Result<User, OperationError> GetUserInOrganization(Guid organizationUuid, Guid userUuid);
    }
}