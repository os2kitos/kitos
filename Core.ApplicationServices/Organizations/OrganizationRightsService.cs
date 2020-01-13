﻿using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.Organizations
{
    public class OrganizationRightsService : IOrganizationRightsService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IGenericRepository<OrganizationRight> _organizationRightRepository;
        private readonly IOrganizationalUserContext _userContext;

        public OrganizationRightsService(
            IAuthorizationContext authorizationContext, 
            IGenericRepository<OrganizationRight> organizationRightRepository,
            IOrganizationalUserContext userContext)
        {
            _authorizationContext = authorizationContext;
            _organizationRightRepository = organizationRightRepository;
            _userContext = userContext;
        }

        public Result<OrganizationRight, OperationFailure> RemoveRole(int organizationId, int userId, OrganizationRole roleId)
        {
            var right = _organizationRightRepository.Get(r => r.OrganizationId == organizationId && r.Role == roleId && r.UserId == userId).FirstOrDefault();

            return RemoveRight(right);
        }

        public Result<OrganizationRight, OperationFailure> RemoveRole(int rightId)
        {
            var right = _organizationRightRepository.GetByKey(rightId);

            return RemoveRight(right);
        }

        private Result<OrganizationRight, OperationFailure> RemoveRight(OrganizationRight right)
        {
            if (right == null)
            {
                return Result<OrganizationRight, OperationFailure>.Failure(OperationFailure.NotFound);
            }

            if (!_authorizationContext.AllowDelete(right))
            {
                return Result<OrganizationRight, OperationFailure>.Failure(OperationFailure.Forbidden);
            }

            if (right.Role == OrganizationRole.GlobalAdmin && right.UserId == _userContext.UserId)
            {
                return Result<OrganizationRight, OperationFailure>.Failure(OperationFailure.Conflict);
            }

            _organizationRightRepository.DeleteByKey(right.Id);
            _organizationRightRepository.Save();

            return Result<OrganizationRight, OperationFailure>.Success(right);
        }
    }
}
