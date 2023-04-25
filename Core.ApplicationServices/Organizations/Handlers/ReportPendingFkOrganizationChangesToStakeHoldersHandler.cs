using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Commands;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices.Context;
using Core.DomainServices.Organizations;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.Organizations.Handlers
{
    public class ReportPendingFkOrganizationChangesToStakeHoldersHandler : ICommandHandler<ReportPendingFkOrganizationChangesToStakeHolders, Maybe<OperationError>>

    {
        private readonly IStsOrganizationUnitService _stsOrganizationUnitService;
        private readonly ILogger _logger;
        private readonly ITransactionManager _transactionManager;
        private readonly IOperationClock _operationClock;
        private readonly IDatabaseControl _databaseControl;
        private readonly IDomainEvents _domainEvents;

        public ReportPendingFkOrganizationChangesToStakeHoldersHandler(
            IStsOrganizationUnitService stsOrganizationUnitService,
            ILogger logger,
            ITransactionManager transactionManager,
            IOperationClock operationClock,
            IDatabaseControl databaseControl,
            IDomainEvents domainEvents)
        {
            _stsOrganizationUnitService = stsOrganizationUnitService;
            _logger = logger;
            _transactionManager = transactionManager;
            _operationClock = operationClock;
            _databaseControl = databaseControl;
            _domainEvents = domainEvents;
        }

        public Maybe<OperationError> Execute(ReportPendingFkOrganizationChangesToStakeHolders command)
        {
            using var transaction = _transactionManager.Begin();

            //Load the external tree if not already provided
            var organization = command.Organization;
            var connection = command.Connection;
            var organizationTree = _stsOrganizationUnitService.ResolveOrganizationTree(organization);

            if (organizationTree.Failed)
            {
                var error = organizationTree.Error;
                _logger.Error("Unable to resolve external org tree for organization with uuid {uuid}. Failed with: {code}:{detail}:{message}", command.Organization.Uuid, error.FailureType, error.Detail, error.Message);
                return new OperationError($"Failed to resolve org tree:{error.Message.GetValueOrFallback("")}:{error.Detail:G}:{error.FailureType:G}", error.FailureType);
            }

            //Compute any pending changes
            var now = _operationClock.Now;
            var logEntriesResult = organization
                .ComputeExternalOrganizationHierarchyUpdateConsequences
                (
                    OrganizationUnitOrigin.STS_Organisation,
                    organizationTree.Value,
                    connection.SynchronizationDepth.FromNullableValueType()
                )
                .Select(consequences => consequences.ToLogEntries(Maybe<ActiveUserIdContext>.None, _operationClock.Now));

            if (logEntriesResult.Failed)
            {
                var error = logEntriesResult.Error;
                _logger.Error("Unable to compute changes org tree for organization with uuid {uuid}. Failed with: {code}:{message}", command.Organization.Uuid, error.FailureType, error.Message);
                return new OperationError($"Failed to compute changes to org tree:{error.Message.GetValueOrFallback("")}:{error.FailureType:G}", error.FailureType);
            }

            //Make sure date of latest check is updated and raise the change resolution event
            var logEntries = logEntriesResult.Value;
            connection.UpdateDateOfLatestUpdateBySubscriptionCheck(now);
            _domainEvents.Raise(new PendingExternalOrganizationUpdatesResolved(organization, logEntries));

            _databaseControl.SaveChanges();
            transaction.Commit();
            return Maybe<OperationError>.None;
        }
    }
}
