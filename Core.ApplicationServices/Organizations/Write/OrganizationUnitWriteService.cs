using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Infrastructure.Services.DataAccess;
using System;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Organizations.Write
{
    public class OrganizationUnitWriteService : IOrganizationUnitWriteService
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IDatabaseControl _databaseControl;
        private readonly IOrganizationUnitService _organizationUnitService;
        private readonly IAuthorizationContext _authorizationContext;

        public OrganizationUnitWriteService(ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IOrganizationUnitService organizationUnitService,
            IAuthorizationContext authorizationContext,
            IDatabaseControl databaseControl)
        {
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _organizationUnitService = organizationUnitService;
            _authorizationContext = authorizationContext;
            _databaseControl = databaseControl;
        }

        public Result<OrganizationUnit, OperationError> Create(Guid organizationUuid, OrganizationUnitUpdateParameters parameters)
        {
            using var transaction = _transactionManager.Begin();
            
            var parentUuidMaybe = parameters.ParentUuid.NewValue;
            if (parentUuidMaybe.IsNone)
            {
                return new OperationError("No parentUuid specified", OperationFailure.BadInput);
            }

            var parentUuid = parentUuidMaybe.Value;
            var name = parameters.Name.NewValue;
            var origin = parameters.Origin.NewValue;

            MarkCreateParametersAsChanged(parameters);

            var result = _organizationUnitService.Create(organizationUuid, parentUuid, name, origin)
                .Bind(unit => Update(unit, parameters));

            if (result.Ok)
            {
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }

            return result;
        }

        private static void MarkCreateParametersAsChanged(OrganizationUnitUpdateParameters parameters)
        {
            parameters.Origin = OptionalValueChange<OrganizationUnitOrigin>.None;
            parameters.Name = OptionalValueChange<string>.None;
            parameters.ParentUuid = OptionalValueChange<Maybe<Guid>>.None;
        }

        private Result<OrganizationUnit, OperationError> Update(OrganizationUnit unit, OrganizationUnitUpdateParameters parameters)
        {
            using var transaction = _transactionManager.Begin();

            var result = WithWriteAccess(unit)
                .Bind(orgUnit => PerformUpdates(orgUnit, parameters));

            if (result.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(result.Value));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }

            return result;
        }

        private static Result<OrganizationUnit, OperationError> PerformUpdates(OrganizationUnit organizationUnit, OrganizationUnitUpdateParameters parameters)
        {
            //Optionally apply changes across the entire update specification
            return organizationUnit.WithOptionalUpdate(parameters.Name, (unit, name) => unit.UpdateName(name))
                .Bind(unit => unit.WithOptionalUpdate(parameters.Origin, (orgUnit, origin) => orgUnit.Origin = origin))
                .Bind(unit => unit.WithOptionalUpdate(parameters.ParentUuid, UpdateParentUnit));
        }

        private static Result<OrganizationUnit, OperationError> UpdateParentUnit(OrganizationUnit organizationUnit, Maybe<Guid> parentUuid)
        {
            if (parentUuid.IsNone)
            {
                organizationUnit.ResetParent();
                return organizationUnit;
            }
            var parentUuidValue = parentUuid.Value;
            var parentUnit = organizationUnit.Organization.GetOrganizationUnit(parentUuidValue);
            if (parentUnit.IsNone)
                return new OperationError($"Organization Unit with uuid: {parentUuidValue} was not found",
                    OperationFailure.NotFound);

            return parentUnit.Value.AddChild(organizationUnit).Match<Result<OrganizationUnit, OperationError>>(error => error, () => organizationUnit);
        }

        private Result<OrganizationUnit, OperationError> WithWriteAccess(OrganizationUnit unit)
        {
            return _authorizationContext.AllowModify(unit) ? unit : new OperationError(OperationFailure.Forbidden);
        }

    }
}
