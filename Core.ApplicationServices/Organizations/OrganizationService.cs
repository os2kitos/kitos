using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.ApplicationServices.Model.Shared;
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
using dk.nita.saml20.Schema.Metadata;
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

        public Result<bool, OperationError> CanActiveUserModifyCvr(Guid organizationUuid)
        {
            return GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Select(_ => _userContext.IsGlobalAdmin());
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

        public Result<Organization, OperationError> UpdateOrganizationMasterData(Guid organizationUuid, OrganizationMasterDataUpdateParameters parameters)
        {
            using var transaction = _transactionManager.Begin();

            var organizationResult = GetOrganizationAndAuthorizeModification(organizationUuid);

            if (organizationResult.Failed) return organizationResult.Error;

            var modifiedOrganizationResult = ModifyOrganization(organizationResult.Value, parameters);

            return modifiedOrganizationResult.Match(
                organization =>
                {
                    _repository.Update(organization);
                    _domainEvents.Raise(new EntityUpdatedEvent<Organization>(organization));
                    transaction.Commit();

                    var updatedOrganizationResult = _repository.GetByUuid(organizationUuid);
                    return updatedOrganizationResult.Match<Result<Organization, OperationError>>(
                        updatedOrganization => updatedOrganization,
                        () => new OperationError(OperationFailure.NotFound)
                    );
                },
                error =>
                {
                    transaction.Rollback();
                    return error;
                }
            );

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

        public Result<ResourcePermissionsResult, OperationError> GetPermissions(Guid organizationUuid)
        {
            return GetOrganization(organizationUuid)
                .Transform(result => ResourcePermissionsResult.FromResolutionResult(result, _authorizationContext));
        }

        public GridPermissions GetGridPermissions(int orgId)
        {
            return new GridPermissions
            {
                ConfigModificationPermission = HasRole(orgId, OrganizationRole.LocalAdmin)
            };
        }

        public Result<OrganizationMasterDataRoles, OperationError> GetOrganizationMasterDataRoles(Guid organizationUuid)
        {
            var organizationDbIdMaybe= _identityResolver.ResolveDbId<Organization>(organizationUuid);
            if (organizationDbIdMaybe.IsNone) return new OperationError(OperationFailure.BadInput);
            var orgId = organizationDbIdMaybe.Value;

            var contactPersonMaybe = _contactPersonRepository.AsQueryable()
                .FirstOrNone(cp => cp.OrganizationId.Equals(orgId));

            var dataResponsibleMaybe = _dataResponsibleRepository.AsQueryable()
                .FirstOrNone(dr => dr.OrganizationId.Equals(orgId));

            var dataProtectionAdvisorMaybe = _dataProtectionAdvisorRepository.AsQueryable()
                .FirstOrNone(dpa => dpa.OrganizationId.Equals(orgId));

            return new OrganizationMasterDataRoles
            {
                OrganizationUuid = organizationUuid,
                ContactPerson = contactPersonMaybe.Value,
                DataResponsible = dataResponsibleMaybe.Value,
                DataProtectionAdvisor = dataProtectionAdvisorMaybe.Value
            };
        }

        public Result<OrganizationMasterDataRoles, OperationError> UpsertOrganizationMasterDataRoles(Guid organizationUuid,
            OrganizationMasterDataRolesUpdateParameters updateParameters)
        {
            using var transaction = _transactionManager.Begin();

            var organizationDbIdMaybe = _identityResolver.ResolveDbId<Organization>(organizationUuid);
            if (organizationDbIdMaybe.IsNone) return new OperationError(OperationFailure.BadInput);
            var orgId = organizationDbIdMaybe.Value;

            var modifiedContactPersonResult =
                AuthorizeModificationAndModifyContactPerson(orgId,
                    updateParameters.ContactPerson);

            var upsertedContactPersonResult = modifiedContactPersonResult.Match(
                contactPerson =>
                {
                    _contactPersonRepository.Update(contactPerson);
                    _domainEvents.Raise(new EntityUpdatedEvent<ContactPerson>(contactPerson));

                    var updatedContactPersonMaybe = _contactPersonRepository.AsQueryable()
                        .FirstOrNone(cp => cp.OrganizationId.Equals(orgId));

                    return updatedContactPersonMaybe.Match<Result<ContactPerson, OperationError>>(
                        updatedContactPerson => updatedContactPerson,
                        () => new OperationError(OperationFailure.NotFound)); 
                },
                error => error
                );
            if (upsertedContactPersonResult.Failed) return ConcludeMasterDataRolesUpdate(upsertedContactPersonResult.Error, transaction);

            var modifiedDataResponsibleResult =
                AuthorizeModificationAndModifyDataResponsible(orgId, updateParameters.DataResponsible);

            var updatedDataResponsibleResult = modifiedDataResponsibleResult.Match(
                dataResponsible =>
                {
                    _dataResponsibleRepository.Update(dataResponsible);
                    _domainEvents.Raise(new EntityUpdatedEvent<DataResponsible>(dataResponsible));

                    var updatedDataResponsibleMaybe = _dataResponsibleRepository.AsQueryable()
                        .FirstOrNone(dr => dr.OrganizationId.Equals(orgId));

                    return updatedDataResponsibleMaybe.Match<Result<DataResponsible, OperationError>>(
                        updatedDataResponsible => updatedDataResponsible,
                        () => new OperationError(OperationFailure.NotFound));
                },
                error => error);
            if (updatedDataResponsibleResult.Failed) return ConcludeMasterDataRolesUpdate(updatedDataResponsibleResult.Error, transaction);

            var modifiedDataProtectionAdvisorResult = AuthorizeModificationAndModifyDataProtectionAdvisor(orgId,
                updateParameters.DataProtectionAdvisor);

            var updatedDataProtectionAdvisorResult = modifiedDataProtectionAdvisorResult.Match(
                dataProtectionAdvisor =>
                {
                    _dataProtectionAdvisorRepository.Update(dataProtectionAdvisor);
                    _domainEvents.Raise(new EntityUpdatedEvent<DataProtectionAdvisor>(dataProtectionAdvisor));

                    var updatedDataProtectionAdvisorMaybe = _dataProtectionAdvisorRepository.AsQueryable()
                        .FirstOrNone(dr => dr.OrganizationId.Equals(orgId));

                    return updatedDataProtectionAdvisorMaybe.Match<Result<DataProtectionAdvisor, OperationError>>(
                        updatedDataProtectionAdvisor => updatedDataProtectionAdvisor,
                        () => new OperationError(OperationFailure.NotFound));
                },
                error => error);
            if (updatedDataProtectionAdvisorResult.Failed) return ConcludeMasterDataRolesUpdate(updatedDataProtectionAdvisorResult.Error, transaction);

            var roles = new OrganizationMasterDataRoles()
            {
                OrganizationUuid = organizationUuid,
                ContactPerson = upsertedContactPersonResult.Value,
                DataProtectionAdvisor = updatedDataProtectionAdvisorResult.Value,
                DataResponsible = updatedDataResponsibleResult.Value
            };
            return ConcludeMasterDataRolesUpdate(roles, transaction);
        }

        private Result<OrganizationMasterDataRoles, OperationError> ConcludeMasterDataRolesUpdate(Result<OrganizationMasterDataRoles, OperationError>  result, IDatabaseTransaction transaction)
        {
            if (result.Ok) transaction.Commit();
            else transaction.Rollback();
            return result;
        }

        private Result<ContactPerson, OperationError> AuthorizeModificationAndModifyContactPerson(
            int organizationId, Maybe<ContactPersonUpdateParameters> parameters)
        {
            var existingContactPersonMaybe = _contactPersonRepository.AsQueryable()
                .FirstOrNone(cp => cp.OrganizationId.Equals(organizationId));
            var existingContactPerson = existingContactPersonMaybe.HasValue 
                ? existingContactPersonMaybe.Value
                : CreateContactPerson(organizationId);

            var allowModify = _authorizationContext.AllowModify(existingContactPerson);
            if (!allowModify) return new OperationError(OperationFailure.Forbidden);

            return parameters.HasValue ? ModifyContactPerson(existingContactPerson, parameters.Value) : existingContactPerson;
        }

        private ContactPerson CreateContactPerson(int orgId)
        {
            var newContactPerson = new ContactPerson() { OrganizationId = orgId };
            _contactPersonRepository.Insert(newContactPerson);
            _domainEvents.Raise(new EntityCreatedEvent<ContactPerson>(newContactPerson));
            return newContactPerson;
        }

        private Result<DataProtectionAdvisor, OperationError> AuthorizeModificationAndModifyDataProtectionAdvisor(
            int organizationId, Maybe<DataProtectionAdvisorUpdateParameters> parameters)
        {
            var existingDataProtectionAdvisorMaybe = _dataProtectionAdvisorRepository.AsQueryable()
                .FirstOrNone(cp => cp.OrganizationId.Equals(organizationId));

            var existingDataProtectionAdvisor = existingDataProtectionAdvisorMaybe.HasValue
                ? existingDataProtectionAdvisorMaybe.Value
                : CreateDataProtectionAdvisor(organizationId);

            var allowModify = _authorizationContext.AllowModify(existingDataProtectionAdvisor);
            if (!allowModify) return new OperationError(OperationFailure.Forbidden);

            return parameters.HasValue ? ModifyDataProtectionAdvisor(existingDataProtectionAdvisor, parameters.Value) : existingDataProtectionAdvisor;
        }

        private DataProtectionAdvisor CreateDataProtectionAdvisor(int orgId)
        {
            var newDataProtectionAdvisor = new DataProtectionAdvisor() { OrganizationId = orgId };
            _dataProtectionAdvisorRepository.Insert(newDataProtectionAdvisor);
            _domainEvents.Raise(new EntityCreatedEvent<DataProtectionAdvisor>(newDataProtectionAdvisor));
            return newDataProtectionAdvisor;
        }

        private Result<DataResponsible, OperationError> AuthorizeModificationAndModifyDataResponsible(
            int organizationId, Maybe<DataResponsibleUpdateParameters> parameters)
        {
            var existingDataResponsibleMaybe = _dataResponsibleRepository.AsQueryable()
                .FirstOrNone(cp => cp.OrganizationId.Equals(organizationId));

            var existingDataResponsible = existingDataResponsibleMaybe.HasValue
                ? existingDataResponsibleMaybe.Value
                : CreateDataResponsible(organizationId);

            var allowModify = _authorizationContext.AllowModify(existingDataResponsible);
            if (!allowModify) return new OperationError(OperationFailure.Forbidden);

            return parameters.HasValue ? ModifyDataResponsible(existingDataResponsible, parameters.Value) : existingDataResponsible;
        }

        private DataResponsible CreateDataResponsible(int orgId)
        {
            var newDataResponsible = new DataResponsible() { OrganizationId = orgId };
            _dataResponsibleRepository.Insert(newDataResponsible);
            _domainEvents.Raise(new EntityCreatedEvent<DataResponsible>(newDataResponsible));
            return newDataResponsible;
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

        private Result<Organization, OperationError> GetOrganizationAndAuthorizeModification(Guid organizationUuid)
        {
            return GetOrganization(organizationUuid)
                .Match
                (
                    organization =>
                        !_authorizationContext.AllowModify(organization)
                            ? new OperationError(OperationFailure.Forbidden)
                            : Result<Organization, OperationError>.Success(organization),
                    error => error
                );
        }

        private static Result<Organization, OperationError> ModifyOrganization(Organization organization,
            OrganizationMasterDataUpdateParameters parameters)
        {
            organization.Cvr = parameters.Cvr?.NewValue;
            organization.Adress = parameters.Address?.NewValue;
            organization.Email = parameters.Email?.NewValue;
            organization.Phone = parameters.Phone?.NewValue;
            return organization;

        } 
        
        private static Result<ContactPerson, OperationError> ModifyContactPerson(ContactPerson contactPerson, ContactPersonUpdateParameters parameters)
        {
            contactPerson.Email = parameters.Email?.NewValue;
            contactPerson.Name = parameters.Name?.NewValue;
            contactPerson.LastName = parameters.LastName?.NewValue;
            contactPerson.PhoneNumber = parameters.PhoneNumber?.NewValue;
            return contactPerson;
        }

        private static Result<DataResponsible, OperationError> ModifyDataResponsible(DataResponsible dataResponsible, DataResponsibleUpdateParameters parameters)
        {
            dataResponsible.Email = parameters.Email?.NewValue;
            dataResponsible.Name = parameters.Name?.NewValue;
            dataResponsible.Cvr = parameters.Cvr?.NewValue;
            dataResponsible.Adress = parameters.Address?.NewValue;
            dataResponsible.Phone = parameters.Phone?.NewValue;
            return dataResponsible;
        }

        private static Result<DataProtectionAdvisor, OperationError> ModifyDataProtectionAdvisor(DataProtectionAdvisor dataProtectionAdvisor, DataProtectionAdvisorUpdateParameters parameters)
        {
            dataProtectionAdvisor.Email = parameters.Email?.NewValue;
            dataProtectionAdvisor.Name = parameters.Name?.NewValue;
            dataProtectionAdvisor.Cvr = parameters.Cvr?.NewValue;
            dataProtectionAdvisor.Adress = parameters.Address?.NewValue;
            dataProtectionAdvisor.Phone = parameters.Phone?.NewValue;
            return dataProtectionAdvisor;
        }
    }
}
