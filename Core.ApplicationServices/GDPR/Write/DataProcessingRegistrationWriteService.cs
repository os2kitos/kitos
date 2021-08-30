using System;
using System.Collections.Generic;
using System.Data;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Infrastructure.Soap.STSAdresse;
using Serilog;

namespace Core.ApplicationServices.GDPR.Write
{
    public class DataProcessingRegistrationWriteService : IDataProcessingRegistrationWriteService
    {
        private readonly IDataProcessingRegistrationApplicationService _applicationService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly ILogger _logger;
        private readonly IDomainEvents _domainEvents;
        private readonly ITransactionManager _transactionManager;
        private readonly IDatabaseControl _databaseControl;

        public DataProcessingRegistrationWriteService(
            IDataProcessingRegistrationApplicationService applicationService,
            IEntityIdentityResolver entityIdentityResolver,
            ILogger logger,
            IDomainEvents domainEvents,
            ITransactionManager transactionManager,
            IDatabaseControl databaseControl)
        {
            _applicationService = applicationService;
            _entityIdentityResolver = entityIdentityResolver;
            _logger = logger;
            _domainEvents = domainEvents;
            _transactionManager = transactionManager;
            _databaseControl = databaseControl;
        }

        public Result<DataProcessingRegistration, OperationError> Create(Guid organizationUuid, DataProcessingRegistrationModificationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

            var orgId = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);

            if (orgId.IsNone)
            {
                _logger.Error("Failed to retrieve organization with id {uuid}");
                return new OperationError($"Unable to resolve Organization with uuid{organizationUuid}", OperationFailure.BadInput);
            }
            if (parameters.Name.IsUnchanged)
                return new OperationError("Name must be provided", OperationFailure.BadInput);

            var name = parameters.Name.NewValue;

            parameters.Name = OptionalValueChange<string>.None; //Remove from change set. It is set during creation

            var creationResult = _applicationService
                .Create(orgId.Value, name)
                .Bind(createdSystemUsage => Update(() => createdSystemUsage, parameters));

            if (creationResult.Ok)
            {
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return creationResult;
        }

        public Result<DataProcessingRegistration, OperationError> Update(Guid dataProcessingRegistrationUuid, DataProcessingRegistrationModificationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return Update(() => _applicationService.GetByUuid(dataProcessingRegistrationUuid), parameters);
        }

        private Result<DataProcessingRegistration, OperationError> Update(Func<Result<DataProcessingRegistration, OperationError>> getDpr, DataProcessingRegistrationModificationParameters parameters)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

            var result = getDpr()
                .Bind(systemUsage => PerformUpdates(systemUsage, parameters));

            if (result.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEvent<DataProcessingRegistration>(result.Value));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return result;
        }

        private Result<DataProcessingRegistration, OperationError> PerformUpdates(DataProcessingRegistration dpr, DataProcessingRegistrationModificationParameters parameters)
        {
            //Optionally apply changes across the entire update specification
            return dpr
                .WithOptionalUpdate(parameters.Name, (registration, changedName) => _applicationService.UpdateName(registration.Id, changedName))
                .Bind(registration => registration.WithOptionalUpdate(parameters.General, UpdateGeneralData));
        }

        private Result<DataProcessingRegistration, OperationError> UpdateGeneralData(DataProcessingRegistration dpr, UpdatedDataProcessingRegistrationGeneralDataParameters parameters)
        {
            return dpr
                .WithOptionalUpdate(parameters.DataResponsibleUuid, UpdateDataResponsible)
                .Bind(r => r.WithOptionalUpdate(parameters.DataResponsibleRemark, (registration, remark) => _applicationService.UpdateDataResponsibleRemark(registration.Id, remark)))
                .Bind(r => r.WithOptionalUpdate(parameters.IsAgreementConcluded, (registration, concluded) => _applicationService.UpdateIsAgreementConcluded(registration.Id, concluded ?? YesNoIrrelevantOption.UNDECIDED)))
                .Bind(r => r.WithOptionalUpdate(parameters.IsAgreementConcludedRemark, (registration, concludedRemark) => _applicationService.UpdateAgreementConcludedRemark(registration.Id, concludedRemark)))
                .Bind(r => r.WithOptionalUpdate(parameters.AgreementConcludedAt, (registration, concludedAt) => _applicationService.UpdateAgreementConcludedAt(registration.Id, concludedAt)))
                .Bind(r => r.WithOptionalUpdate(parameters.BasisForTransferUuid, UpdateBasisForTransfer))
                .Bind(r => r.WithOptionalUpdate(parameters.TransferToInsecureThirdCountries, (registration, newValue) => _applicationService.UpdateTransferToInsecureThirdCountries(registration.Id, newValue ?? YesNoUndecidedOption.Undecided)))
                .Bind(r => r.WithOptionalUpdate(parameters.InsecureCountriesSubjectToDataTransferUuids, UpdateInsecureCountriesSubjectToDataTransfer))
                .Bind(r => r.WithOptionalUpdate(parameters.DataProcessorUuids, UpdateDataProcessors))
                .Bind(r => r.WithOptionalUpdate(parameters.HasSubDataProcesors, (registration, newValue) => _applicationService.SetSubDataProcessorsState(registration.Id, newValue ?? YesNoUndecidedOption.Undecided)))
                .Bind(r => r.WithOptionalUpdate(parameters.SubDataProcessorUuids, UpdateSubDataProcessors));
        }

        private Maybe<OperationError> UpdateSubDataProcessors(DataProcessingRegistration dpr, Maybe<IEnumerable<Guid>> organizationUuids)
        {
            throw new NotImplementedException();
        }

        private Maybe<OperationError> UpdateDataProcessors(DataProcessingRegistration dpr, Maybe<IEnumerable<Guid>> organizationUuids)
        {
            throw new NotImplementedException();
        }

        private Maybe<OperationError> UpdateInsecureCountriesSubjectToDataTransfer(DataProcessingRegistration dpr, Maybe<IEnumerable<Guid>> countryOptionUuids)
        {
            throw new NotImplementedException();
        }

        private Maybe<OperationError> UpdateBasisForTransfer(DataProcessingRegistration dpr, Guid? basisForTransferUuid)
        {
            throw new NotImplementedException();
        }

        private Maybe<OperationError> UpdateDataResponsible(DataProcessingRegistration dpr, Guid? dataResponsibleUuid)
        {
            throw new NotImplementedException();
        }

        public Maybe<OperationError> Delete(Guid dataProcessingRegistrationUuid)
        {
            var dbId = _entityIdentityResolver.ResolveDbId<DataProcessingRegistration>(dataProcessingRegistrationUuid);

            if (dbId == null)
                return new OperationError(OperationFailure.NotFound);

            return _applicationService
                .Delete(dbId.Value)
                .Match(_ => Maybe<OperationError>.None, error => error);
        }
    }
}
