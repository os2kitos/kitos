using System;
using System.Data;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
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
        private readonly IDomainEvents _domainEvents;
        private readonly ILogger _logger;

        public ItSystemUsageWriteService(
            IItSystemUsageService systemUsageService,
            ITransactionManager transactionManager,
            IItSystemService systemService,
            IOrganizationService organizationService,
            IDomainEvents domainEvents,
            ILogger logger)
        {
            _systemUsageService = systemUsageService;
            _transactionManager = transactionManager;
            _systemService = systemService;
            _organizationService = organizationService;
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
            throw new NotImplementedException();
            //TODO: Domain events in this method
        }

        public Maybe<OperationError> Delete(Guid itSystemUsageUuid)
        {
            throw new NotImplementedException();
        }
    }
}
