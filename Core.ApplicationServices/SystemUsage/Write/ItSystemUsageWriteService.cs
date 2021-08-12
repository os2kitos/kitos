using System;
using System.Data;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices.Options;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Serilog;

namespace Core.ApplicationServices.SystemUsage.Write
{
    public class ItSystemUsageWriteService : IItSystemUsageWriteService
    {
        private readonly IItSystemUsageService _systemUsageService;
        private readonly ITransactionManager _transactionManager;
        private readonly IItSystemService _systemService;
        private readonly IOrganizationService _organizationService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOptionsService<ItSystemUsage, ItSystemCategories> _systemCategoriesOptionsService;
        private readonly IDomainEvents _domainEvents;
        private readonly ILogger _logger;

        public ItSystemUsageWriteService(
            IItSystemUsageService systemUsageService,
            ITransactionManager transactionManager,
            IItSystemService systemService,
            IOrganizationService organizationService,
            IAuthorizationContext authorizationContext,
            IOptionsService<ItSystemUsage, ItSystemCategories> systemCategoriesOptionsService,
            IDomainEvents domainEvents,
            ILogger logger)
        {
            _systemUsageService = systemUsageService;
            _transactionManager = transactionManager;
            _systemService = systemService;
            _organizationService = organizationService;
            _authorizationContext = authorizationContext;
            _systemCategoriesOptionsService = systemCategoriesOptionsService;
            _domainEvents = domainEvents;
            _logger = logger;
        }

        public Result<ItSystemUsage, OperationError> Create(SystemUsageCreationParameters parameters)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            var systemResult = _systemService.GetSystem(parameters.SystemUuid);
            if (systemResult.Failed)
            {
                _logger.Error("Failed to retrieve itSystem with id {uuid}. Error {error}", parameters.SystemUuid, systemResult.Error.ToString());
                return new OperationError("Unable to resolve IT-System:" + systemResult.Error.Message, systemResult.Error.FailureType);
            }

            var organizationResult = _organizationService.GetOrganization(parameters.OrganizationUuid);
            if (organizationResult.Failed)
            {
                _logger.Error("Failed to retrieve organization with id {uuid}. Error {error}", parameters.OrganizationUuid, organizationResult.Error.ToString());
                return new OperationError("Unable to resolve IT-System:" + organizationResult.Error.Message, organizationResult.Error.FailureType);
            }

            var creationResult = _systemUsageService
                .CreateNew(systemResult.Value.Id, organizationResult.Value.Id)
                .Bind(createdSystemUsage => Update(createdSystemUsage.Uuid, parameters.AdditionalValues));

            if (creationResult.Ok)
            {
                transaction.Commit();
            }

            return creationResult;
        }

        public Result<ItSystemUsage, OperationError> Update(Guid systemUsageUuid, SystemUsageUpdateParameters parameters)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

            var result = _systemUsageService
                .GetByUuid(systemUsageUuid)
                .Bind(WithWriteAccess)
                .Bind(systemUsage => PerformUpdates(systemUsage, parameters));

            if (result.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(result.Value));
                transaction.Commit();
            }

            return result;
        }

        private Result<ItSystemUsage, OperationError> PerformUpdates(ItSystemUsage systemUsage, SystemUsageUpdateParameters parameters)
        {
            //Optionally apply changes across the entire update specification
            return WithOptionalUpdate(systemUsage, parameters.GeneralProperties, PerformGeneralDataPropertiesUpdate)
                    .Bind(usage => WithOptionalUpdate(usage, parameters.GeneralProperties, PerformRoleAssignmentUpdates));
        }//TODO: PLaceholder methods so that we can work in parallel

        private Result<ItSystemUsage, OperationError> PerformGeneralDataPropertiesUpdate(ItSystemUsage itSystemUsage, Maybe<UpdatedSystemUsageGeneralProperties> newPropertyValues)
        {
            //if not defined we fallback to an empty input (because the values were provided as None = reset)
            var generalProperties = newPropertyValues.Match(definedValues => definedValues, () => new UpdatedSystemUsageGeneralProperties());
            return WithOptionalUpdate(itSystemUsage, generalProperties.LocalCallName, (systemUsage, localCallName) => systemUsage.UpdateLocalCallName(localCallName))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.LocalSystemId, (systemUsage, localSystemId) => systemUsage.UpdateLocalSystemId(localSystemId)))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.DataClassificationUuid, UpdateDataClassification))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.Notes, (systemUsage, notes) => systemUsage.Note = notes))
                .Bind(usage => WithOptionalUpdate(usage, generalProperties.SystemVersion, (systemUsage, version) => systemUsage.UpdateSystemVersion(version)));

            //TODO: Add the rest as above

        }

        private Maybe<OperationError> UpdateDataClassification(ItSystemUsage systemUsage, Maybe<Guid> dataClassificationOptionId)
        {
            if (dataClassificationOptionId.IsNone)
            {
                systemUsage.ResetSystemCategories();
                return Maybe<OperationError>.None;
            }

            var optionByUuid = _systemCategoriesOptionsService.GetOptionByUuid(systemUsage.OrganizationId, dataClassificationOptionId.Value);

            if (optionByUuid.IsNone)
                return new OperationError("Invalid option id", OperationFailure.BadInput);

            if (!optionByUuid.Value.available)
                return new OperationError("Option is not available in the organization.", OperationFailure.BadInput);

            return systemUsage.UpdateSystemCategories(optionByUuid.Value.option);
        }

        /*
        public Maybe<ChangedValue<Maybe<(int lower, int? upperBound)>>> NumberOfExpectedUsersInterval { get; set; } = Maybe<ChangedValue<Maybe<(int lower, int? upperBound)>>>.None;
        public Maybe<ChangedValue<Maybe<bool>>> EnforceActive { get; set; } = Maybe<ChangedValue<Maybe<bool>>>.None;
        public Maybe<ChangedValue<Maybe<DateTime>>> ValidFrom { get; set; } = Maybe<ChangedValue<Maybe<DateTime>>>.None;
        public Maybe<ChangedValue<Maybe<DateTime>>> ValidTo { get; set; } = Maybe<ChangedValue<Maybe<DateTime>>>.None;
        public Maybe<ChangedValue<Maybe<Guid>>> MainContractUuid { get; set; } = Maybe<ChangedValue<Maybe<Guid>>>.None;
        public Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>> AssociatedProjectUuids { get; set; } = Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>>.None;
         */

        private Result<ItSystemUsage, OperationError> PerformRoleAssignmentUpdates(ItSystemUsage itSystemUsage, Maybe<UpdatedSystemUsageGeneralProperties> newPropertyValues)
        {
            return itSystemUsage; //TODO: Redefine signature. This method is just here to show how all updates are combined into one

        }

        private static Result<ItSystemUsage, OperationError> WithOptionalUpdate<TValue>(
            ItSystemUsage systemUsage,
            Maybe<ChangedValue<TValue>> optionalUpdate,
            Func<ItSystemUsage, TValue, Result<ItSystemUsage, OperationError>> updateCommand)
        {
            return optionalUpdate
                .Select(changedValue => updateCommand(systemUsage, changedValue.Value))
                .Match(updateResult => updateResult, () => systemUsage);
        }

        private static Result<ItSystemUsage, OperationError> WithOptionalUpdate<TValue>(
            ItSystemUsage systemUsage,
            Maybe<ChangedValue<TValue>> optionalUpdate,
            Func<ItSystemUsage, TValue, Maybe<OperationError>> updateCommand)
        {
            return optionalUpdate
                .Select(changedValue => updateCommand(systemUsage, changedValue.Value).Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage))
                .Match(updateResult => updateResult, () => systemUsage);
        }

        private static Result<ItSystemUsage, OperationError> WithOptionalUpdate<TValue>(
            ItSystemUsage systemUsage,
            Maybe<ChangedValue<TValue>> optionalUpdate,
            Action<ItSystemUsage, TValue> updateCommand)
        {
            return optionalUpdate
                .Select(changedValue =>
                {
                    updateCommand(systemUsage, changedValue.Value);
                    return systemUsage;
                })
                .Match(updateResult => updateResult, () => systemUsage);
        }

        private Result<ItSystemUsage, OperationError> WithWriteAccess(ItSystemUsage systemUsage)
        {
            return _authorizationContext.AllowModify(systemUsage) ? systemUsage : new OperationError(OperationFailure.Forbidden);
        }

        public Maybe<OperationError> Delete(Guid itSystemUsageUuid)
        {
            throw new NotImplementedException();
        }
    }
}
