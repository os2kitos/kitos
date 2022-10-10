﻿using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Users;
using Core.DomainServices.Queries;

namespace Core.ApplicationServices
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
        Result<IQueryable<User>, OperationError> GetUsersInOrganization(Guid organizationUuid, params IDomainQuery<User>[] queries);
        Result<User, OperationError> GetUserInOrganization(Guid organizationUuid, Guid userUuid);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userUuid"></param>
        /// <param name="scopedToOrganizationId">If provided the operation will be scoped to the organization identified by this parameter</param>
        /// <returns></returns>
        Maybe<OperationError> InitiateUserDeletion(Guid userUuid, int? scopedToOrganizationId = null);
        Result<IQueryable<User>, OperationError> SearchAllKitosUsers(params IDomainQuery<User>[] queries);
    }
}