using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Organization;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.DataAccess;

using Serilog;

namespace Core.ApplicationServices.Organizations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IOrganizationRepository _repository;
        private readonly IOrgUnitService _orgUnitService;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly IDomainEvents _domainEvents;
        private readonly IGenericRepository<OrganizationRight> _orgRightRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationalUserContext _userContext;
        private readonly ILogger _logger;
        private readonly ITransactionManager _transactionManager;

        public OrganizationService(
            IGenericRepository<Organization> orgRepository,
            IGenericRepository<OrganizationRight> orgRightRepository,
            IGenericRepository<User> userRepository,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext userContext,
            ILogger logger,
            ITransactionManager transactionManager,
            IOrganizationRepository repository,
            IOrganizationRightsService organizationRightsService,
            IOrgUnitService orgUnitService,
            IDomainEvents domainEvents)
        {
            _orgRepository = orgRepository;
            _orgRightRepository = orgRightRepository;
            _userRepository = userRepository;
            _authorizationContext = authorizationContext;
            _userContext = userContext;
            _logger = logger;
            _transactionManager = transactionManager;
            _repository = repository;
            _orgUnitService = orgUnitService;
            _domainEvents = domainEvents;
            _organizationRightsService = organizationRightsService;
        }

        //returns the default org unit for that user inside that organization
        //or null if none has been chosen
        public OrganizationUnit GetDefaultUnit(Organization organization, User user)
        {
            return _orgRightRepository
                .Get(r => r.OrganizationId == organization.Id && r.UserId == user.Id)
                .Select(r => r.DefaultOrgUnit)
                .FirstOrDefault();
        }

        public void SetDefaultOrgUnit(User user, int orgId, int orgUnitId)
        {
            var right = _orgRightRepository.Get(r => r.UserId == user.Id && r.OrganizationId == orgId).First();
            right.DefaultOrgUnitId = orgUnitId;

            _orgRightRepository.Update(right);
            _orgRightRepository.Save();
        }

        /// <summary>
        /// Remove all organization rights from a user.
        /// </summary>
        /// <param name="organizationId">The organization the user should be removed from.</param>
        /// <param name="userId">The user to be removed.</param>
        public Result<Organization, OperationFailure> RemoveUser(int organizationId, int userId)
        {
            using var transaction = _transactionManager.Begin();
            var organization = _orgRepository.GetByKey(organizationId);
            if (organization == null)
            {
                return OperationFailure.NotFound;
            }

            if (!_authorizationContext.AllowModify(organization))
            {
                return OperationFailure.Forbidden;
            }

            var rights = _orgRightRepository
                .AsQueryable()
                .Where(x => x.UserId == userId && x.OrganizationId == organizationId)
                .AsEnumerable();

            foreach (var right in rights)
            {
                var result = _organizationRightsService.RemoveRole(right.Id);
                if (result.Failed)
                {
                    _logger.Error("Failed to delete right with id {rightId} due to error: {errorCode}", right.Id, result.Error);
                    transaction.Rollback();
                    return Result<Organization, OperationFailure>.Failure(OperationFailure.UnknownError);
                }
            }
            transaction.Commit();

            return organization;
        }

        public bool CanChangeOrganizationType(Organization organization, OrganizationTypeKeys organizationType)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }
            return
                _authorizationContext.AllowModify(organization) &&
                _authorizationContext.HasPermission(new DefineOrganizationTypePermission(organizationType, organization.Id));
        }

        public Result<bool, OperationError> CanActiveUserModifyCvr(Guid organizationUuid)
        {
            var organization = GetOrganization(organizationUuid);

            return _userContext.IsGlobalAdmin();
        }

        public Result<Organization, OperationFailure> CreateNewOrganization(Organization newOrg)
        {
            if (newOrg == null)
            {
                throw new ArgumentNullException(nameof(newOrg));
            }
            var user = _userRepository.GetByKey(_userContext.UserId);

            if (user == null)
            {
                return OperationFailure.Forbidden;
            }

            //Setup defaults
            newOrg.Uuid = newOrg.Uuid == Guid.Empty ? Guid.NewGuid() : newOrg.Uuid;
            newOrg.Config = Config.Default(user);
            newOrg.OrgUnits.Add(new OrganizationUnit
            {
                Name = newOrg.Name,
            });

            if (newOrg.IsCvrInvalid())
            {
                _logger.Error("Invalid cvr {cvr} provided for org with name {name}", newOrg.Cvr, newOrg.Name);
                return OperationFailure.BadInput;
            }

            if (!_userContext.OrganizationIds.Any(id => _authorizationContext.AllowCreate<Organization>(id)))
            {
                return OperationFailure.Forbidden;
            }

            if (newOrg.TypeId > 0)
            {
                var organizationType = (OrganizationTypeKeys)newOrg.TypeId;
                var allowOrganizationTypeCreation = _userContext.OrganizationIds.Any(id => _authorizationContext.HasPermission(new DefineOrganizationTypePermission(organizationType, id)));
                if (!allowOrganizationTypeCreation)
                {
                    return OperationFailure.Forbidden;
                }
            }
            else
            {
                //Invalid org key
                return OperationFailure.BadInput;
            }

            using var transaction = _transactionManager.Begin();

            newOrg = _orgRepository.Insert(newOrg);
            _orgRepository.Save();
            transaction.Commit();
            return newOrg;
        }

        public Result<Organization, OperationError> GetOrganization(Guid organizationUuid, OrganizationDataReadAccessLevel? withMinimumAccessLevel = null)
        {
            return _repository.GetByUuid(organizationUuid).Match<Result<Organization, OperationError>>(organization =>
                {
                    var hasAccess = withMinimumAccessLevel.HasValue
                        ? _authorizationContext.GetOrganizationReadAccessLevel(organization.Id) >= withMinimumAccessLevel.Value
                        : _authorizationContext.AllowReads(organization);

                    if (!hasAccess)
                    {
                        return new OperationError(OperationFailure.Forbidden);
                    }
                    return organization;
                },
                () => new OperationError(OperationFailure.NotFound)
            );
        }

        public Result<IQueryable<Organization>, OperationError> GetAllOrganizations()
        {
            if (_authorizationContext.GetCrossOrganizationReadAccess() != CrossOrganizationDataReadAccessLevel.All)
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            return Result<IQueryable<Organization>, OperationError>.Success(_repository.GetAll());
        }

        public IQueryable<Organization> SearchAccessibleOrganizations(params IDomainQuery<Organization>[] conditions)
        {
            var crossOrganizationReadAccess = _authorizationContext.GetCrossOrganizationReadAccess();

            var domainQueries = conditions.ToList();
            if (crossOrganizationReadAccess < CrossOrganizationDataReadAccessLevel.All)
            {
                //Restrict organization access
                domainQueries =
                    new QueryOrganizationByIdsOrSharedAccess(_userContext.OrganizationIds, crossOrganizationReadAccess == CrossOrganizationDataReadAccessLevel.Public)
                        .WrapAsEnumerable()
                        .Concat(domainQueries)
                        .ToList();
            }

            var query = new IntersectionQuery<Organization>(domainQueries);
            return _repository.GetAll().Transform(query.Apply);
        }

        public Result<IQueryable<OrganizationUnit>, OperationError> GetOrganizationUnits(Guid organizationUuid, params IDomainQuery<OrganizationUnit>[] criteria)
        {
            return GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Select(_orgUnitService.GetOrganizationUnits)
                .Select(new IntersectionQuery<OrganizationUnit>(criteria).Apply);
        }

        public Result<OrganizationUnit, OperationError> GetOrganizationUnit(Guid organizationUnitUuid)
        {
            return
                _orgUnitService
                    .GetOrganizationUnit(organizationUnitUuid)
                    .Match<Result<OrganizationUnit, OperationError>>(
                        unit => _authorizationContext.AllowReads(unit)
                            ? unit
                            : new OperationError(OperationFailure.Forbidden),
                        () => new OperationError(OperationFailure.NotFound));
        }

        public Result<OrganizationRemovalConflicts, OperationError> ComputeOrganizationRemovalConflicts(Guid organizationUuid)
        {
            return GetOrganization(organizationUuid)
                .Bind(WithDeletionAccess)
                .Select(organizationWhichCanBeDeleted =>
                {
                    var systemsWithUsagesOutsideTheOrganization = organizationWhichCanBeDeleted
                        .ItSystems
                        .Where(x => x.Usages.Any(usage => usage.OrganizationId != organizationWhichCanBeDeleted.Id))
                        .ToList();
                    var interfacesExposedOnSystemsOutsideTheOrganization = organizationWhichCanBeDeleted
                        .ItInterfaces
                        .Where(x => x.ExhibitedBy != null && x.ExhibitedBy.ItSystem != null && x.ExhibitedBy.ItSystem.OrganizationId != organizationWhichCanBeDeleted.Id)
                        .ToList();
                    var systemsExposingInterfacesDefinedInOtherOrganizations = organizationWhichCanBeDeleted
                        .ItSystems
                        .Where(x => x.ItInterfaceExhibits.Any(ex => ex.ItInterface != null && ex.ItInterface.OrganizationId != organizationWhichCanBeDeleted.Id))
                        .ToList();
                    var systemsSetAsParentSystemToSystemsInOtherOrganizations = organizationWhichCanBeDeleted
                        .ItSystems
                        .Where(x => x.Children.Any(c => c.OrganizationId != organizationWhichCanBeDeleted.Id))
                        .ToList();
                    var dprInOtherOrganizationsWhereOrgIsDataProcessor = organizationWhichCanBeDeleted
                        .DataProcessorForDataProcessingRegistrations
                        .Where(x => x.OrganizationId != organizationWhichCanBeDeleted.Id)
                        .ToList();
                    var dprInOtherOrganizationsWhereOrgIsSubDataProcessor = organizationWhichCanBeDeleted
                        .SubDataProcessorForDataProcessingRegistrations
                        .Where(x => x.OrganizationId != organizationWhichCanBeDeleted.Id)
                        .ToList();
                    var contractsInOtherOrganizationsWhereOrgIsSupplier = organizationWhichCanBeDeleted
                        .Supplier
                        .Where(x => x.OrganizationId != organizationWhichCanBeDeleted.Id)
                        .ToList();
                    var systemsInOtherOrgsWhereOrgIsRightsHolder = organizationWhichCanBeDeleted
                        .BelongingSystems
                        .Where(x => x.OrganizationId != organizationWhichCanBeDeleted.Id)
                        .ToList();
                    var systemsWhereOrgIsArchiveSupplier = organizationWhichCanBeDeleted
                        .ArchiveSupplierForItSystems
                        .Where(x => x.OrganizationId != organizationWhichCanBeDeleted.Id)
                        .ToList();
                    
                    return new OrganizationRemovalConflicts(
                        systemsWithUsagesOutsideTheOrganization,
                        interfacesExposedOnSystemsOutsideTheOrganization,
                        systemsExposingInterfacesDefinedInOtherOrganizations,
                        systemsSetAsParentSystemToSystemsInOtherOrganizations,
                        dprInOtherOrganizationsWhereOrgIsDataProcessor,
                        dprInOtherOrganizationsWhereOrgIsSubDataProcessor,
                        contractsInOtherOrganizationsWhereOrgIsSupplier,
                        systemsInOtherOrgsWhereOrgIsRightsHolder,
                        systemsWhereOrgIsArchiveSupplier);
                });
        }

        public Maybe<OperationError> RemoveOrganization(Guid uuid, bool enforceDeletion)
        {
            using var transaction = _transactionManager.Begin();
            var organizationWhichCanBeDeleted = GetOrganization(uuid).Bind(WithDeletionAccess);

            if (organizationWhichCanBeDeleted.Failed)
            {
                return organizationWhichCanBeDeleted.Error;
            }

            if (organizationWhichCanBeDeleted.Value.IsDefaultOrganization == true)
            {
                return new OperationError("Cannot delete default organization", OperationFailure.BadInput);
            }

            var conflicts = ComputeOrganizationRemovalConflicts(uuid);
            if (conflicts.Failed)
                return conflicts.Error;

            var conflictsToResolve = conflicts.Value;
            if (conflictsToResolve.Any && !enforceDeletion)
                return new OperationError("Removal conflicts not resolved", OperationFailure.Conflict);

            try
            {
                var organization = organizationWhichCanBeDeleted.Value;
                _domainEvents.Raise(new EntityBeingDeletedEvent<Organization>(organization));
                _orgRepository.DeleteWithReferencePreload(organization);
                _orgRepository.Save();
                transaction.Commit();
            }
            catch (Exception error)
            {
                _logger.Error(error, "Failed while deleting organization with uuid: {uuid}", uuid);
                return new OperationError("Exception during deletion", OperationFailure.UnknownError);
            }
            return Maybe<OperationError>.None;
        }

        public Result<IEnumerable<Organization>, OperationError> GetUserOrganizations(int userId)
        {
            var user = _userRepository.GetByKey(userId);
            if(user == null)
                return Result<IEnumerable<Organization>, OperationError>.Failure(new OperationError($"User with id: {userId} was not found", OperationFailure.NotFound));

            var userOrganizationsIds = user.GetOrganizationIds();

            return Result<IEnumerable<Organization>, OperationError>.Success(_orgRepository.AsQueryable().ByIds(userOrganizationsIds.ToList()));
        }

        private Result<Organization, OperationError> WithDeletionAccess(Organization organization)
        {
            if (_authorizationContext.AllowDelete(organization))
            {
                return organization;
            }

            return new OperationError(OperationFailure.Forbidden);
        }
    }
}
