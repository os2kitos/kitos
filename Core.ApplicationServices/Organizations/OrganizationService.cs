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
using Core.DomainServices.Generic;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Organization;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.DataAccess;

using Serilog;
using Organization = Core.DomainModel.Organization.Organization;

namespace Core.ApplicationServices.Organizations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IOrganizationRepository _repository;
        private readonly IOrgUnitService _orgUnitService;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly IDomainEvents _domainEvents;
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IGenericRepository<OrganizationRight> _orgRightRepository;
        private readonly IGenericRepository<ContactPerson> _contactPersonRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationalUserContext _userContext;
        private readonly ILogger _logger;
        private readonly ITransactionManager _transactionManager;
        private readonly IGenericRepository<DataResponsible> _dataResponsibleRepository;
        private readonly IGenericRepository<DataProtectionAdvisor> _dataProtectionAdvisorRepository;

        public OrganizationService(
            IGenericRepository<Organization> orgRepository,
            IGenericRepository<OrganizationRight> orgRightRepository,
            IGenericRepository<ContactPerson> contactPersonRepository,
            IGenericRepository<User> userRepository,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext userContext,
            ILogger logger,
            ITransactionManager transactionManager,
            IOrganizationRepository repository,
            IOrganizationRightsService organizationRightsService,
            IOrgUnitService orgUnitService,
            IDomainEvents domainEvents,
            IEntityIdentityResolver identityResolver, IGenericRepository<DataResponsible> dataResponsibleRepository,
            IGenericRepository<DataProtectionAdvisor> dataProtectionAdvisorRepository)
        {
            _orgRepository = orgRepository;
            _orgRightRepository = orgRightRepository;
            _contactPersonRepository = contactPersonRepository;
            _userRepository = userRepository;
            _authorizationContext = authorizationContext;
            _userContext = userContext;
            _logger = logger;
            _transactionManager = transactionManager;
            _repository = repository;
            _orgUnitService = orgUnitService;
            _domainEvents = domainEvents;
            _identityResolver = identityResolver;
            _dataResponsibleRepository = dataResponsibleRepository;
            _dataProtectionAdvisorRepository = dataProtectionAdvisorRepository;
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

        public IQueryable<Organization> SearchAccessibleOrganizations(bool onlyWithMembershipAccess, params IDomainQuery<Organization>[] conditions)
        {
            var crossOrganizationReadAccess = _authorizationContext.GetCrossOrganizationReadAccess();

            var initialFilters = new List<IDomainQuery<Organization>>();

            //If user does not have full access to everything, we apply the membership and sharing rules
            if (crossOrganizationReadAccess < CrossOrganizationDataReadAccessLevel.All)
            {
                //If requested only membership or if user cannot access shared orgs, we restrict the dataset to only include the orgs in which the user has a full membership
                if (onlyWithMembershipAccess || crossOrganizationReadAccess < CrossOrganizationDataReadAccessLevel.Public)
                {
                    initialFilters.Add(new QueryByIds<Organization>(_userContext.OrganizationIds));
                }
                else
                {
                    //Refinement to restrict organization access to include only accessible organizations (membership or shared data)
                    initialFilters.Add(new QueryOrganizationByIdsOrSharedAccess(_userContext.OrganizationIds, true));
                }
            }

            var refinements = initialFilters
                .Concat(conditions)
                .ToList();

            var query = new IntersectionQuery<Organization>(refinements);
            return _repository.GetAll().Transform(query.Apply);
        }

        public IQueryable<Organization> SearchAccessibleOrganizations(params IDomainQuery<Organization>[] conditions)
        {
            return SearchAccessibleOrganizations(false, conditions);
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
                        .SubDataProcessorRegistrations
                        .Select(x => x.DataProcessingRegistration)
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
            if (user == null)
                return Result<IEnumerable<Organization>, OperationError>.Failure(new OperationError($"User with id: {userId} was not found", OperationFailure.NotFound));

            var userOrganizationsIds = user.GetOrganizationIds();

            return Result<IEnumerable<Organization>, OperationError>.Success(_orgRepository.AsQueryable().ByIds(userOrganizationsIds.ToList()));
        }

        public Result<OrganizationPermissionsResult, OperationError> GetPermissions(Guid organizationUuid)
        {
            return GetModifyCvrPermission(organizationUuid).Bind(modifyCvr => GetOrganization(organizationUuid)
                .Transform(result =>
                    OrganizationPermissionsResult.FromResolutionResult(result, _authorizationContext,
                        modifyCvr)));
        }

        private Result<bool, OperationError> GetModifyCvrPermission(Guid organizationUuid)
        {
            var result = CanActiveUserModifyCvr(organizationUuid);
            if (result.Failed && result.Error.FailureType == OperationFailure.Forbidden) return false;
            return result;
        }

        public Result<bool, OperationError> CanActiveUserModifyCvr(Guid organizationUuid)
        {
            return GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Select(_ => _userContext.IsGlobalAdmin());
        }

        public Maybe<DataResponsible> GetDataResponsible(int organizationId)
        {
            return _dataResponsibleRepository.AsQueryable()
                .FirstOrNone(cp => cp.OrganizationId.Equals(organizationId));
        }

        public Maybe<ContactPerson> GetContactPerson(int organizationId)
        {
            return _contactPersonRepository.AsQueryable()
                .FirstOrNone(cp => cp.OrganizationId.Equals(organizationId));
        }

        public Maybe<DataProtectionAdvisor> GetDataProtectionAdvisor(int organizationId)
        {
            return _dataProtectionAdvisorRepository.AsQueryable()
                .FirstOrNone(dpa => dpa.OrganizationId.Equals(organizationId));
        }

        public Result<Config, OperationError> GetUIRootConfig(Guid organizationUuid)
        {
            return GetOrganization(organizationUuid)
                .Match(organization =>
                    Result<Config, OperationError>.Success(organization.Config),
                    error => error);
        }

        public GridPermissions GetGridPermissions(int orgId)
        {
            return new GridPermissions
            {
                ConfigModificationPermission = HasRole(orgId, OrganizationRole.LocalAdmin)
            };
        }

        private Result<Organization, OperationError> WithDeletionAccess(Organization organization)
        {
            if (_authorizationContext.AllowDelete(organization))
            {
                return organization;
            }

            return new OperationError(OperationFailure.Forbidden);
        }

        private bool HasRole(int orgId, OrganizationRole role)
        {
            return _userContext.HasRole(orgId, role);
        }
    }
}
