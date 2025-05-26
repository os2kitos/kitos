using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Helpers;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.Commands;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Role;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Organizations
{
    public class OrganizationUnitService : IOrganizationUnitService
    {
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly IItContractService _contractService;
        private readonly IItSystemUsageService _usageService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IDatabaseControl _databaseControl;
        private readonly IGenericRepository<OrganizationUnit> _repository;
        private readonly ICommandBus _commandBus;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit>
            _assignmentService;



        public OrganizationUnitService(IOrganizationService organizationService,
            IOrganizationRightsService organizationRightsService,
            IItContractService contractService,
            IItSystemUsageService usageService,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IDatabaseControl databaseControl,
            IGenericRepository<OrganizationUnit> repository,
            ICommandBus commandBus,
            IGenericRepository<Organization> organizationRepository,
            IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit>
                assignmentService)
        {
            _organizationService = organizationService;
            _organizationRightsService = organizationRightsService;
            _contractService = contractService;
            _usageService = usageService;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _databaseControl = databaseControl;
            _repository = repository;
            _commandBus = commandBus;
            _organizationRepository = organizationRepository;
            _assignmentService = assignmentService;
        }

        public Result<UnitAccessRights, OperationError> GetAccessRights(Guid organizationUuid, Guid unitUuid)
        {
            return _organizationService
                .GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Bind<(Organization organization, OrganizationUnit organizationUnit)>
                (
                    organization =>
                    {
                        var unit = organization.GetOrganizationUnit(unitUuid);
                        if (unit.IsNone)
                        {
                            return new OperationError($"Organization unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);
                        }
                        return (organization, unit.Value);
                    }
                )
                .Select(orgAndUnit => GetAccessRights(orgAndUnit.organization, orgAndUnit.organizationUnit));
        }

        public Result<IEnumerable<UnitAccessRightsWithUnitData>, OperationError> GetAccessRightsByOrganization(Guid organizationUuid)
        {
            return _organizationService
                .GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Select
                (
                    organization =>
                    {
                        var units = organization.GetAllOrganizationUnits();
                        return (organization, units);
                    }
                )
                .Select(orgAndUnits =>
                    orgAndUnits
                        .units
                        .Select(unit => new UnitAccessRightsWithUnitData(unit, GetAccessRights(orgAndUnits.organization, unit)))
                        .ToList()
                        .AsEnumerable()
                );
        }

        public Maybe<OperationError> Delete(Guid organizationUuid, Guid unitUuid)
        {
            using var transaction = _transactionManager.Begin();
            var deleteResult = GetOrganizationAndAuthorizeModification(organizationUuid)
                .Bind(organization => CombineWithOrganizationUnit(unitUuid, organization))
                .Bind(WithDeletionPermission)
                .Bind(DeleteOrganizationUnit);

            if (deleteResult.Failed)
            {
                transaction.Rollback();
            }
            else
            {
                _databaseControl.SaveChanges();
                transaction.Commit();
            }
            return deleteResult.MatchFailure();
        }

        public Result<OrganizationUnit, OperationError> Create(Guid organizationUuid, Guid parentUuid,
            string name, OrganizationUnitOrigin origin)
        {
            using var transaction = _transactionManager.Begin();

            var result = _organizationService
                .GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Bind(WithUnitCreateAccess)
                .Bind(organization => _organizationService.GetOrganizationUnit(parentUuid)
                    .Select(unit => (organization, parentUnit: unit)))
                .Bind(values => AddUnitToOrganization(values.organization, values.parentUnit, name, origin));

            if (result.Ok)
            {
                var newUnit = result.Value;
                var organization = newUnit.Organization;
                _repository.Insert(newUnit);
                _domainEvents.Raise(new EntityCreatedEvent<OrganizationUnit>(newUnit));

                _organizationRepository.Update(organization);
                _domainEvents.Raise(new EntityUpdatedEvent<Organization>(organization));

                _databaseControl.SaveChanges();
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }

            return result;
        }

        public Maybe<OperationError> DeleteRegistrations(Guid organizationUuid, Guid unitUuid, OrganizationUnitRegistrationChangeParameters parameters)
        {
            return ModifyRegistrations(organizationUuid, unitUuid, (_, unit) =>
            {
                return _organizationRightsService.RemoveUnitRightsByIds(organizationUuid, unitUuid, parameters.OrganizationUnitRights)
                    .Match
                    (
                        error => error,
                        () => RemovePaymentResponsibleUnits(parameters.PaymentRegistrationDetails)
                    )
                    .Match
                    (
                        error => error,
                        () => RemoveContractRegistrations(parameters.ItContractRegistrations)
                    )
                    .Match
                    (
                        error => error,
                        () => RemoveSystemResponsibleRegistrations(parameters.ResponsibleSystems)
                    )
                    .Match
                    (
                        error => error,
                        () => RemoveSystemRelevantUnits(parameters.RelevantSystems, unitUuid)
                    )
                    .Match
                    (
                        error => error,
                        () => Result<OrganizationUnit, OperationError>.Success(unit)
                    );
            }).MatchFailure();
        }

        public Maybe<OperationError> DeleteRegistrations(Guid organizationUuid, Guid unitUuid)
        {
            return ModifyRegistrations(organizationUuid, unitUuid, (_, unit) =>
            {
                return GetRegistrations(organizationUuid, unitUuid)
                    .Bind<OrganizationUnit>
                    (
                        details =>
                        {
                            var error = DeleteRegistrations(organizationUuid, unitUuid, ToChangeParametersFromRegistrationDetails(details));
                            if (error.HasValue)
                            {
                                return error.Value;
                            }

                            return unit;
                        }
                    );
            }).MatchFailure();
        }

        public Maybe<OperationError> TransferRegistrations(Guid organizationUuid, Guid unitUuid, Guid targetUnitUuid, OrganizationUnitRegistrationChangeParameters parameters)
        {
            return ModifyRegistrations(organizationUuid, unitUuid, (organization, unit) =>
            {
                var targetUnitResult = organization.GetOrganizationUnit(targetUnitUuid);
                if (targetUnitResult.IsNone)
                {
                    return new OperationError($"Unit with uuid: {targetUnitUuid} was not found",
                        OperationFailure.NotFound);
                }

                var targetUnit = targetUnitResult.Value;

                if (!_authorizationContext.AllowModify(targetUnit))
                {
                    return new OperationError(OperationFailure.Forbidden);
                }

                var error = _organizationRightsService.TransferUnitRightsByIds(organizationUuid, unitUuid,
                        targetUnitUuid, parameters.OrganizationUnitRights)
                    .Match
                    (
                        error => error,
                        () => TransferPayments(targetUnitUuid, parameters.PaymentRegistrationDetails)
                    )
                    .Match
                    (
                        error => error,
                        () => TransferContractRegistrations(targetUnitUuid, parameters.ItContractRegistrations)
                    )
                    .Match
                    (
                        error => error,
                        () => TransferSystemResponsibleRegistrations(targetUnitUuid, parameters.ResponsibleSystems)
                    )
                    .Match
                    (
                        error => error,
                        () => TransferSystemRelevantRegistrations(unitUuid, targetUnitUuid, parameters.RelevantSystems)
                    );

                if (error.HasValue)
                {
                    return error.Value;
                }

                _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(targetUnitResult.Value));

                return Result<OrganizationUnit, OperationError>.Success(unit);
            }).MatchFailure();
        }

        public Result<Organization, OperationError> GetOrganizationAndAuthorizeModification(Guid uuid)
        {
            return _organizationService.GetOrganization(uuid, OrganizationDataReadAccessLevel.All)
                .Match
                (
                    organization =>
                        _authorizationContext.AllowModify(organization) == false
                            ? new OperationError("User is not allowed to modify the organization", OperationFailure.Forbidden)
                            : Result<Organization, OperationError>.Success(organization),
                    error => error
                );
        }

        public Result<IEnumerable<OrganizationUnitRight>, OperationError> GetRightsOfUnitSubtree(Guid organizationUuid,
            Guid organizationUnitUuid)
        {
            var unitResult = _organizationService.GetOrganizationUnit(organizationUnitUuid);
            if (unitResult.Failed)
            {
                return unitResult.Error;
            }

            var unit = unitResult.Value;
            var rights = GetAllSubunitRightsOfUnit(unit);
            return rights;
        }

        public Result<OrganizationUnitRight, OperationError> CreateRoleAssignment(Guid organizationUnitUuid, Guid roleUuid, Guid userUuid)
        {
            return ModifyUnitRights(organizationUnitUuid, unit => _assignmentService.AssignRole(unit, roleUuid, userUuid));
        }

        public Result<OrganizationUnit, OperationError> CreateBulkRoleAssignment(Guid organizationUnitUuid, IEnumerable<UserRolePair> assignments)
        {
            return ModifyUnitRights<OrganizationUnit>(organizationUnitUuid, unit =>
            {
                var assignmentList = assignments.ToList();
                var roleAssignments = GetRoleAssignmentUpdates(unit, assignmentList);
                if (roleAssignments.Failed)
                {
                    return roleAssignments.Error;
                }

                _assignmentService.BatchUpdateRoles(unit, roleAssignments.Value.Select(pair => (pair.RoleUuid, pair.UserUuid))
                    .ToList());

                return unit;
            });
        }

        public Result<OrganizationUnitRight, OperationError> DeleteRoleAssignment(Guid organizationUnitUuid, Guid roleUuid, Guid userUuid)
        {
            return ModifyUnitRights(organizationUnitUuid, unit => _assignmentService.RemoveRole(unit, roleUuid, userUuid));
        }

        private static Result<IEnumerable<UserRolePair>, OperationError> GetRoleAssignmentUpdates(OrganizationUnit unit, IEnumerable<UserRolePair> assignments)
        {
            var existingRoles = RoleMappingHelper.ExtractAssignedRoles(unit);
            var newRoles = assignments.ToList();
            return existingRoles.Any(newRoles.Contains) ?
                 new OperationError("Role assignment exists", OperationFailure.Conflict) :
                 Result<IEnumerable<UserRolePair>, OperationError>.Success(existingRoles.Concat(newRoles));

        }

        private Result<T, OperationError> ModifyUnitRights<T>(Guid organizationUnitUuid,
            Func<OrganizationUnit, Result<T, OperationError>> mutation)
        {
            var unitResult = _organizationService.GetOrganizationUnit(organizationUnitUuid);
            if (unitResult.Failed)
            {
                return unitResult.Error;
            }
            var unit = unitResult.Value;
            if (!_authorizationContext.AllowModify(unit))
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            return mutation(unit);
        }

        private List<OrganizationUnitRight> GetAllSubunitRightsOfUnit(OrganizationUnit rootUnit)
        {
            var rights = new List<OrganizationUnitRight>();
            rights.AddRange(rootUnit.Rights);
            foreach (var childUnit in rootUnit.Children)
            {
                var childUnitTreeRights = GetAllSubunitRightsOfUnit(childUnit);
                rights.AddRange(childUnitTreeRights);
            }
            return rights;
        }

        private Result<TSuccess, OperationError> Modify<TSuccess>(Guid organizationUuid, Guid unitUuid, Func<Organization, OrganizationUnit, Result<TSuccess, OperationError>> mutation)
        {
            using var transaction = _transactionManager.Begin();

            var organizationResult = GetOrganizationAndAuthorizeModification(organizationUuid);

            if (organizationResult.Failed)
            {
                return organizationResult.Error;
            }
            var organization = organizationResult.Value;

            var unitResult = organization.GetOrganizationUnit(unitUuid);
            if (unitResult.IsNone)
            {
                return new OperationError($"Unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);
            }
            var unit = unitResult.Value;

            if (!_authorizationContext.AllowModify(unit))
                return new OperationError(OperationFailure.Forbidden);

            var mutationResult = mutation(organization, unit);

            if (mutationResult.Failed)
            {
                transaction.Rollback();
            }
            else
            {
                _repository.Update(unit);
                _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(unitResult.Value));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return mutationResult;
        }

        private static Result<OrganizationUnit, OperationError> AddUnitToOrganization(Organization organization,
            OrganizationUnit parentUnit, string name, OrganizationUnitOrigin origin)
        {
            var newUnit = new OrganizationUnit
            {
                Name = name,
                Origin = origin,
                Organization = organization,
            };

            return organization.AddOrganizationUnit(newUnit, parentUnit)
                .Match<Result<OrganizationUnit, OperationError>>(error => error, () => newUnit);
        }

        private Result<TSuccess, OperationError> ModifyRegistrations<TSuccess>(Guid organizationId, Guid unitUuid, Func<Organization, OrganizationUnit, Result<TSuccess, OperationError>> mutation)
        {
            return Modify(organizationId, unitUuid,
                (org, orgUnit) => _authorizationContext.HasPermission(new BulkAdministerOrganizationUnitRegistrations(org.Id))
                    ? mutation(org, orgUnit)
                    : new OperationError("User is not authorized for bulk permission administration", OperationFailure.Forbidden)
                );
        }

        private UnitAccessRights GetAccessRights(Organization organization, OrganizationUnit unit)
        {
            if (!_authorizationContext.AllowModify(unit))
                return UnitAccessRights.ReadOnly();

            const bool canBeModified = true;
            var canBeRenamed = false;
            const bool canInfoAdditionalfieldsBeModified = true;
            var canBeRearranged = false;
            var canBeDeleted = false;

            if (unit.IsNativeKitosUnit())
            {
                canBeRenamed = true;

                if (organization.GetRoot() != unit)
                {
                    canBeRearranged = true;
                    canBeDeleted = true;
                }
            }
            if (!_authorizationContext.AllowDelete(unit))
            {
                canBeDeleted = false;
            }

            return new UnitAccessRights(canBeRead: true, canBeModified, canBeRenamed, canInfoAdditionalfieldsBeModified, canBeRearranged, canBeDeleted, _authorizationContext.HasPermission(new BulkAdministerOrganizationUnitRegistrations(organization.Id)));
        }

        private Maybe<OperationError> RemovePaymentResponsibleUnits(IEnumerable<PaymentChangeParameters> payments)
        {
            foreach (var payment in payments)
            {
                var removeInternalPaymentsError = _contractService.RemovePaymentResponsibleUnits(payment.ItContractId, true, payment.InternalPayments);
                if (removeInternalPaymentsError.HasValue)
                    return removeInternalPaymentsError.Value;

                var removeExternalPaymentsError = _contractService.RemovePaymentResponsibleUnits(payment.ItContractId, false, payment.ExternalPayments);
                if (removeExternalPaymentsError.HasValue)
                    return removeExternalPaymentsError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveContractRegistrations(IEnumerable<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                var deleteError = _contractService.RemoveResponsibleUnit(contractId);
                if (deleteError.HasValue)
                    return deleteError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveSystemRelevantUnits(IEnumerable<int> systemIds, Guid unitUuid)
        {
            foreach (var systemId in systemIds)
            {
                var deleteError = _usageService.RemoveRelevantUnit(systemId, unitUuid);
                if (deleteError.HasValue)
                    return deleteError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveSystemResponsibleRegistrations(IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var deleteError = _usageService.RemoveResponsibleUsage(systemId);
                if (deleteError.HasValue)
                    return deleteError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferPayments(Guid targetUnitUuid, IEnumerable<PaymentChangeParameters> payments)
        {
            foreach (var payment in payments)
            {
                var transferInternalPaymentsError = _contractService.TransferPayments(payment.ItContractId, targetUnitUuid, true, payment.InternalPayments);
                if (transferInternalPaymentsError.HasValue)
                    return transferInternalPaymentsError.Value;

                var transferExternalPaymentsError = _contractService.TransferPayments(payment.ItContractId, targetUnitUuid, false, payment.ExternalPayments);
                if (transferExternalPaymentsError.HasValue)
                    return transferExternalPaymentsError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferSystemResponsibleRegistrations(Guid targetUnitUuid, IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var transferError = _usageService.TransferResponsibleUsage(systemId, targetUnitUuid);
                if (transferError.HasValue)
                    return transferError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferContractRegistrations(Guid targetUnitUuid, IEnumerable<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                var transferError = _contractService.SetResponsibleUnit(contractId, targetUnitUuid);
                if (transferError.HasValue)
                    return transferError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferSystemRelevantRegistrations(Guid unitUuid, Guid targetUnitUuid, IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var transferError = _usageService.TransferRelevantUsage(systemId, unitUuid, targetUnitUuid);
                if (transferError.HasValue)
                    return transferError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private static OrganizationUnitRegistrationChangeParameters ToChangeParametersFromRegistrationDetails(
            OrganizationUnitRegistrationDetails unitRegistrations)
        {
            var itContractRegistrations = unitRegistrations.ItContractRegistrations.Select(x => x.Id).ToList();
            var organizationUnitRights = unitRegistrations.OrganizationUnitRights.Select(x => x.Id).ToList();
            var paymentRegistrationDetails = unitRegistrations.PaymentRegistrationDetails.Select
            (x =>
                new PaymentChangeParameters
                (
                    x.ItContract.Id,
                    x.InternalPayments.Select(ip => ip.Id).ToList(),
                    x.ExternalPayments.Select(ep => ep.Id).ToList()
                )
            ).ToList();
            var relevantSystems = unitRegistrations.RelevantSystems.Select(x => x.Id).ToList();
            var responsibleSystems = unitRegistrations.ResponsibleSystems.Select(x => x.Id).ToList();

            return new OrganizationUnitRegistrationChangeParameters(organizationUnitRights, itContractRegistrations, paymentRegistrationDetails, responsibleSystems, relevantSystems);
        }

        private Result<OrganizationUnit, OperationError> DeleteOrganizationUnit((Organization organization, OrganizationUnit organizationUnit) orgAndUnit)
        {
            var (organization, organizationUnit) = orgAndUnit;
            var deleteCommand = new RemoveOrganizationUnitRegistrationsCommand(organization, organizationUnit);
            var deleteRegistrationsError = _commandBus.Execute<RemoveOrganizationUnitRegistrationsCommand, Maybe<OperationError>>(deleteCommand);
            if (deleteRegistrationsError.HasValue)
            {
                return deleteRegistrationsError.Value;
            }

            _domainEvents.Raise(new EntityBeingDeletedEvent<OrganizationUnit>(organizationUnit));
            var error = organization.DeleteOrganizationUnit(organizationUnit);
            if (error.HasValue)
            {
                return error.Value;
            }

            _repository.DeleteWithReferencePreload(organizationUnit);
            return organizationUnit;
        }

        private Result<(Organization organization, OrganizationUnit organizationUnit), OperationError> WithDeletionPermission((Organization organization, OrganizationUnit organizationUnit) orgAndUnit)
        {
            var accessRights = GetAccessRights(orgAndUnit.organization, orgAndUnit.organizationUnit);
            if (accessRights.CanBeDeleted == false)
                return new OperationError("Not authorized to delete org unit", OperationFailure.Forbidden);

            return orgAndUnit;
        }

        private static Result<(Organization organization, OrganizationUnit organizationUnit), OperationError> CombineWithOrganizationUnit(Guid unitUuid, Organization organization)
        {
            var organizationUnit = organization.GetOrganizationUnit(unitUuid);
            if (organizationUnit.IsNone)
                return new OperationError(OperationFailure.NotFound);
            return (organization, organizationUnit.Value);
        }

        public Result<OrganizationUnitRegistrationDetails, OperationError> GetRegistrations(Guid organizationUuid, Guid unitUuid)
        {
            return _organizationService
                .GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Bind<OrganizationUnit>
                (
                    organization =>
                    {
                        var unit = organization.GetOrganizationUnit(unitUuid);
                        if (unit.IsNone)
                            return new OperationError($"Organization unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);

                        return unit.Value;
                    }
                )
                .Select(unit => unit.GetUnitRegistrations());
        }

        private Result<Organization, OperationError> WithUnitCreateAccess(Organization organization)
        {
            return _authorizationContext.AllowCreate<OrganizationUnit>(organization.Id)
                ? organization
                : new OperationError(OperationFailure.Forbidden);
        }
    }
}
