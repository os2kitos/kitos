using System;
using System.Data;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Serilog;

namespace Core.ApplicationServices.GDPR.Write
{
    public class DataProcessingRegistrationWriteService : IDataProcessingRegistrationWriteService
    {
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationApplicationService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly ILogger _logger;
        private readonly IDomainEvents _domainEvents;
        private readonly ITransactionManager _transactionManager;
        private readonly IDatabaseControl _databaseControl;

        public DataProcessingRegistrationWriteService(
            IDataProcessingRegistrationApplicationService dataProcessingRegistrationApplicationService,
            IEntityIdentityResolver entityIdentityResolver,
            ILogger logger,
            IDomainEvents domainEvents,
            ITransactionManager transactionManager,
            IDatabaseControl databaseControl)
        {
            _dataProcessingRegistrationApplicationService = dataProcessingRegistrationApplicationService;
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
                return new OperationError($"Name must be provided", OperationFailure.BadInput);
            var name = parameters.Name.NewValue;

            var creationResult = _dataProcessingRegistrationApplicationService.Create(orgId.Value, name)
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
            return Update(() => _dataProcessingRegistrationApplicationService.GetByUuid(dataProcessingRegistrationUuid), parameters);
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
            //TODO:
            return dpr;
        }

        public Maybe<OperationError> Delete(Guid dataProcessingRegistrationUuid)
        {
            var dbId = _entityIdentityResolver.ResolveDbId<DataProcessingRegistration>(dataProcessingRegistrationUuid);

            if (dbId == null)
                return new OperationError(OperationFailure.NotFound);

            return _dataProcessingRegistrationApplicationService
                .Delete(dbId.Value)
                .Match(_ => Maybe<OperationError>.None, error => error);
        }
    }
}
