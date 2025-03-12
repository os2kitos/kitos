using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Users;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries;

namespace Core.ApplicationServices
{
    public interface IUserService : IDisposable
    {
        User AddUser(User user, bool sendMailOnCreation, int orgId, bool newUI);

        void UpdateUser(User user, bool? sendMailOnUpdate, int? scopedToOrganizationId, bool newUI);
        void IssueAdvisMail(User user, bool reminder, int orgId, bool newUI);
        PasswordResetRequest IssuePasswordReset(User user, string subject, string content, bool newUI);
        PasswordResetRequest GetPasswordReset(string hash);
        void ResetPassword(PasswordResetRequest passwordResetRequest, string newPassword);
        Result<IQueryable<User>, OperationError> GetUsersWithCrossOrganizationPermissions();
        Result<IQueryable<User>, OperationError> GetUsersWithRoleAssignedInAnyOrganization(OrganizationRole role);
        Result<IQueryable<User>, OperationError> GetUsersInOrganization(Guid organizationUuid, params IDomainQuery<User>[] queries);
        IQueryable<User> GetUsers(params IDomainQuery<User>[] queries);
        Result<IEnumerable<Organization>, OperationError> GetUserOrganizations(Guid userUuid);
        Result<User, OperationError> GetUserInOrganization(Guid organizationUuid, Guid userUuid);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userUuid"></param>
        /// <param name="scopedToOrganizationId">If provided the operation will be scoped to the organization identified by this parameter</param>
        /// <returns></returns>
        Maybe<OperationError> DeleteUser(Guid userUuid, int? scopedToOrganizationId = null);
        Result<IQueryable<User>, OperationError> SearchAllKitosUsers(params IDomainQuery<User>[] queries);
        Result<UserAdministrationPermissions, OperationError> GetAdministrativePermissions(Guid organizationUuid);
        bool IsEmailInUse(string email);
        Result<User, OperationError> GetUserByEmail(Guid organizationUuid, string email);
        Result<User, OperationError> GetUserByUuid(Guid userUuid);
        Result<User, OperationError> GetGlobalAdmin();
    }
}