﻿using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainModel.Organization.DomainEvents;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.Organizations
{
    public class OrganizationRightsService : IOrganizationRightsService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IGenericRepository<OrganizationRight> _organizationRightRepository;
        private readonly IGenericRepository<OrganizationUnitRight> _unitRightRepository;
        private readonly IOrganizationalUserContext _userContext;
        private readonly IDomainEvents _domainEvents;
        private readonly ILogger _logger;
        private readonly ITransactionManager _transactionManager;

        public OrganizationRightsService(IAuthorizationContext authorizationContext,
            IGenericRepository<OrganizationRight> organizationRightRepository,
            IOrganizationalUserContext userContext,
            IDomainEvents domainEvents, ILogger logger,
            IGenericRepository<OrganizationUnitRight> unitRightRepository,
            ITransactionManager transactionManager)
        {
            _authorizationContext = authorizationContext;
            _organizationRightRepository = organizationRightRepository;
            _userContext = userContext;
            _domainEvents = domainEvents;
            _logger = logger;
            _unitRightRepository = unitRightRepository;
            _transactionManager = transactionManager;
        }

        public Result<OrganizationRight, OperationFailure> AssignRole(int organizationId, int userId, OrganizationRole roleId)
        {
            var right = new OrganizationRight
            {
                OrganizationId = organizationId,
                Role = roleId,
                UserId = userId
            };

            if (!_authorizationContext.AllowCreate<OrganizationRight>(organizationId, right))
            {
                return OperationFailure.Forbidden;
            }

            var existingRight = _organizationRightRepository.AsQueryable().FirstOrDefault(x => x.OrganizationId == organizationId && x.UserId == userId && x.Role == roleId);
            if (existingRight != null)
            {
                _logger.Warning("Attempt to assign existing organization ({orgId}) role ({roleId}) to user ({userId}). Existing right ({rightId}) returned", organizationId, roleId, userId, existingRight.Id);
                return right;
            }

            right = _organizationRightRepository.Insert(right);
            _organizationRightRepository.Save();
            _domainEvents.Raise(new AdministrativeAccessRightsChanged(userId));
            return right;
        }

        public Result<OrganizationRight, OperationFailure> RemoveRole(int organizationId, int userId, OrganizationRole roleId)
        {
            var right = _organizationRightRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .Where(r => r.Role == roleId && r.UserId == userId)
                .FirstOrDefault();

            return RemoveRight(right);
        }

        public Result<OrganizationRight, OperationFailure> RemoveRole(int rightId)
        {
            var right = _organizationRightRepository.GetByKey(rightId);

            return RemoveRight(right);
        }

        public Maybe<OperationError> RemoveUnitRightsByIds(int organizationId, IEnumerable<int> rightIds)
        {
            using var transaction = _transactionManager.Begin();

            //var result = CanModifyOrganization(organizationId);
            //if (result.Failed || result.Value == false)
            //{
            //    return new OperationError("User is not allowed to modify the organization", OperationFailure.Forbidden);
            //}

            var rightsToDelete = new List<OrganizationUnitRight>();
            foreach (var rightId in rightIds)
            {
                var unitRight = _unitRightRepository.GetByKey(rightId);
                if (unitRight == null)
                    return new OperationError($"Unit right with id: {rightId} not found", OperationFailure.NotFound);
                if (!_authorizationContext.AllowModify(unitRight.Object))
                    return new OperationError("User is not allowed to perform this operation", OperationFailure.Forbidden);
                
                rightsToDelete.Add(unitRight);
            }
            var userIds = rightsToDelete.Select(x => x.UserId).ToList();

            _unitRightRepository.RemoveRange(rightsToDelete);
            foreach (var userId in userIds.Distinct())
            {
                _domainEvents.Raise(new AdministrativeAccessRightsChanged(userId));
            }
            _unitRightRepository.Save();
            transaction.Commit();

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> TransferUnitRightsByIds(int organizationId, int targetUnitId, IEnumerable<int> rightIds)
        {
            using var transaction = _transactionManager.Begin();

            //var result = CanModifyOrganization(organizationId);
            //if (result.Failed || result.Value == false)
            //{
            //    return new OperationError("User is not allowed to modify the organization", OperationFailure.Forbidden);
            //}

            foreach (var rightId in rightIds)
            {
                var unitRight = _unitRightRepository.GetByKey(rightId);
                if (unitRight == null)
                {
                    return new OperationError(OperationFailure.NotFound);
                }

                if (!_authorizationContext.AllowModify(unitRight.Object))
                {
                    return new OperationError(OperationFailure.Forbidden);
                }
                

                unitRight.ObjectId = targetUnitId;

                _unitRightRepository.Update(unitRight);
                _domainEvents.Raise(new AdministrativeAccessRightsChanged(unitRight.UserId));
            }
            _unitRightRepository.Save();
            transaction.Commit();
            return Maybe<OperationError>.None;
        }

        /*private Result<bool, OperationError> CanModifyOrganization(int id)
        {
            return _identityResolver.ResolveUuid<Organization>(id)
                .Select(x => _organizationService.GetOrganization(x, OrganizationDataReadAccessLevel.All))
                .GetValueOrDefault()
                .Match
                (
                    organization => Result<bool, OperationError>.Success(_authorizationContext.AllowModify(organization)),
                    error => error
                );
        }*/

        private Result<OrganizationRight, OperationFailure> RemoveRight(OrganizationRight right)
        {
            if (right == null)
            {
                return OperationFailure.NotFound;
            }

            if (!_authorizationContext.AllowDelete(right))
            {
                return OperationFailure.Forbidden;
            }

            if (right.Role == OrganizationRole.GlobalAdmin && right.UserId == _userContext.UserId)
            {
                //Only other global admins should do this. Otherwise the system could end up without a global admin
                return OperationFailure.Conflict;
            }

            _organizationRightRepository.DeleteByKey(right.Id);
            _organizationRightRepository.Save();
            _domainEvents.Raise(new AdministrativeAccessRightsChanged(right.UserId));

            return right;
        }
    }
}
