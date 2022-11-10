using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainModel.Organization.DomainEvents;
using Core.DomainServices;
using Core.DomainServices.Extensions;
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
        private readonly IGenericRepository<Organization> _organizationRepository;

        public OrganizationRightsService(IAuthorizationContext authorizationContext,
            IGenericRepository<OrganizationRight> organizationRightRepository,
            IOrganizationalUserContext userContext,
            IDomainEvents domainEvents, ILogger logger,
            IGenericRepository<OrganizationUnitRight> unitRightRepository,
            ITransactionManager transactionManager,
            IGenericRepository<Organization> organizationRepository)
        {
            _authorizationContext = authorizationContext;
            _organizationRightRepository = organizationRightRepository;
            _userContext = userContext;
            _domainEvents = domainEvents;
            _logger = logger;
            _unitRightRepository = unitRightRepository;
            _transactionManager = transactionManager;
            _organizationRepository = organizationRepository;
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

        public Maybe<OperationError> RemoveUnitRightsByIds(Guid organizationUuid, Guid unitUuid, IEnumerable<int> rightIds)
        {
            using var transaction = _transactionManager.Begin();

            var unitResult = GetOrganizationUnitByUuidAndAuthorizeModification(organizationUuid, unitUuid);
            if (unitResult.Failed)
            {
                return unitResult.Error;
            }
            var unit = unitResult.Value;

            var rightsToDelete = new List<OrganizationUnitRight>();
            foreach (var rightId in rightIds)
            {
                var organizationUnitRightResult = unit.GetRight(rightId);
                if (organizationUnitRightResult.IsNone)
                {
                    return new OperationError($"Organization unit right with id: {rightId} was not found", OperationFailure.NotFound);
                }
                var rightToRemove = organizationUnitRightResult.Value;
                
                var result = unit.RemoveRole(rightToRemove.Role, rightToRemove.User);
                if (result.Failed)
                {
                    return result.Error;
                }

                rightsToDelete.Add(rightToRemove);
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

        public Maybe<OperationError> TransferUnitRightsByIds(Guid organizationUuid, Guid unitUuid, Guid targetUnitUuid, IEnumerable<int> rightIds)
        {
            using var transaction = _transactionManager.Begin();

            var organizationResult = GetOrganizationAndAuthorizeModification(organizationUuid);
            if (organizationResult.Failed)
            {
                return organizationResult.Error;
            }
            var organization = organizationResult.Value;

            var unitResult = GetOrganizationUnitAndAuthorizeModification(organization, unitUuid);
            if (unitResult.Failed)
            {
                return unitResult.Error;
            }

            var targetUnitResult = GetOrganizationUnitAndAuthorizeModification(organization, targetUnitUuid);
            if (targetUnitResult.Failed)
            {
                return targetUnitResult.Error;
            }

            var currentUnit = unitResult.Value;
            var targetUnit = targetUnitResult.Value;

            var rightsToDelete = new List<OrganizationUnitRight>();
            foreach (var rightId in rightIds)
            {
                var organizationUnitRightResult = currentUnit.GetRight(rightId);

                if (organizationUnitRightResult.IsNone)
                {
                    return new OperationError($"Organization unit right with id: {rightId} was not found", OperationFailure.NotFound);
                }
                var right = organizationUnitRightResult.Value;

                var removeRightResult = currentUnit.RemoveRole(right.Role, right.User);
                if (removeRightResult.Failed)
                {
                    return removeRightResult.Error;
                }
                var removedRight = removeRightResult.Value;
                rightsToDelete.Add(removedRight);

                var assignRightResult = targetUnit.AssignRole(removedRight.Role, removedRight.User);
                if (assignRightResult.Failed)
                {
                    return assignRightResult.Error;
                }
                
                _domainEvents.Raise(new AdministrativeAccessRightsChanged(removedRight.UserId));
            }

            _unitRightRepository.RemoveRange(rightsToDelete);
            _unitRightRepository.Save();
            transaction.Commit();

            return Maybe<OperationError>.None;
        }

        private Result<OrganizationUnit, OperationError> GetOrganizationUnitByUuidAndAuthorizeModification(Guid organizationUuid, Guid unitUuid)
        {
            return GetOrganizationAndAuthorizeModification(organizationUuid)
                .Bind(organization => GetOrganizationUnitAndAuthorizeModification(organization, unitUuid));
        }

        private Result<OrganizationUnit, OperationError> GetOrganizationUnitAndAuthorizeModification(Organization organization, Guid unitUuid)
        {
            return organization.GetOrganizationUnit(unitUuid)
                .Match
                (
                    WithModificationAccess, 
                    () => new OperationError($"Unit with uuid: {unitUuid} was not found", OperationFailure.NotFound)
                );
        }

        private Result<Organization, OperationError> GetOrganizationAndAuthorizeModification(Guid uuid)
        {
            var organization = _organizationRepository.AsQueryable().FirstOrDefault(x => x.Uuid == uuid);
            return organization == null 
                ? new OperationError($"Organization with uuid: {uuid} was not found", OperationFailure.NotFound) 
                : WithModificationAccess(organization);
        }

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

        private Result<T, OperationError> WithModificationAccess<T>(T entity) where T : IEntity
        {
            return _authorizationContext.AllowModify(entity) ? Result<T, OperationError>.Success(entity) : new OperationError(OperationFailure.Forbidden);
        }
    }
}
