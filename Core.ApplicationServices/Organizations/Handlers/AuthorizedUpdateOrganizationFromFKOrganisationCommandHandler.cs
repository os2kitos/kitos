using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Commands;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using System;
using System.Linq;
using Core.ApplicationServices.Extensions;
using Core.DomainServices;
using Core.DomainServices.Organizations;
using Infrastructure.Services.DataAccess;
using Serilog;
using Core.DomainServices.Context;
using Core.DomainServices.Time;

namespace Core.ApplicationServices.Organizations.Handlers
{
    public class AuthorizedUpdateOrganizationFromFKOrganisationCommandHandler : ICommandHandler<AuthorizedUpdateOrganizationFromFKOrganisationCommand, Maybe<OperationError>>
    {
        private readonly IStsOrganizationUnitService _stsOrganizationUnitService;
        private readonly IGenericRepository<OrganizationUnit> _organizationUnitRepository;
        private readonly ILogger _logger;
        private readonly IDomainEvents _domainEvents;
        private readonly IDatabaseControl _databaseControl;
        private readonly ITransactionManager _transactionManager;
        private readonly Maybe<ActiveUserIdContext> _userContext;
        private readonly IOperationClock _operationClock;
        private readonly IGenericRepository<StsOrganizationChangeLog> _stsChangeLogRepository;

        public AuthorizedUpdateOrganizationFromFKOrganisationCommandHandler(
            IStsOrganizationUnitService stsOrganizationUnitService,
            IGenericRepository<OrganizationUnit> organizationUnitRepository,
            ILogger logger,
            IDomainEvents domainEvents,
            IDatabaseControl databaseControl,
            ITransactionManager transactionManager,
            Maybe<ActiveUserIdContext> userContext,
            IOperationClock operationClock,
            IGenericRepository<StsOrganizationChangeLog> stsChangeLogRepository)
        {
            _stsOrganizationUnitService = stsOrganizationUnitService;
            _organizationUnitRepository = organizationUnitRepository;
            _logger = logger;
            _domainEvents = domainEvents;
            _databaseControl = databaseControl;
            _transactionManager = transactionManager;
            _userContext = userContext;
            _operationClock = operationClock;
            _stsChangeLogRepository = stsChangeLogRepository;
        }

        //TODO: Migrate tests for this
        public Maybe<OperationError> Execute(AuthorizedUpdateOrganizationFromFKOrganisationCommand command)
        {
            var organization = command.Organization;
            using var transaction = _transactionManager.Begin();
            try
            {
                //Load the external tree
                var organizationTree = _stsOrganizationUnitService.ResolveOrganizationTree(organization);
                if (organizationTree.Failed)
                {
                    var error = organizationTree.Error;
                    _logger.Error("Unable to resolve external org tree for organization with uuid {uuid}. Failed with: {code}:{detail}:{message}", command.Organization.Uuid, error.FailureType, error.Detail, error.Message);
                    return new OperationError($"Failed to resolve org tree:{error.Message.GetValueOrFallback("")}:{error.Detail:G}:{error.FailureType:G}", error.FailureType);
                }

                //Import the external tree into the organization
                //TODO: See if addition can be handled in the update method in stead!
                var updateResult = organization.UpdateConnectionToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, organizationTree.Value, command.SynchronizationDepth, command.SubscribeToChanges);
                if (updateResult.Failed)
                {
                    var error = updateResult.Error;
                    _logger.Error("Failed importing org tree for organization with uuid {uuid}. Failed with: {code}:{message}", command.Organization.Uuid, error.FailureType, error.Message);
                    transaction.Rollback();
                    return new OperationError($"Failed to import org tree:{error.Message.GetValueOrFallback("")}:{error.FailureType:G}", error.FailureType);
                }

                //React on import consequences
                var consequences = updateResult.Value;
                if (consequences.DeletedExternalUnitsBeingDeleted.Any())
                {
                    _organizationUnitRepository.RemoveRange(consequences.DeletedExternalUnitsBeingDeleted.Select(x => x.organizationUnit).ToList());
                }
                foreach (var (affectedUnit, _, _) in consequences.OrganizationUnitsBeingRenamed)
                {
                    _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(affectedUnit));
                }

                if (IsBackgroundImport())
                {
                    //TODO: Fix that
                    organization.StsOrganizationConnection.DateOfLatestCheckBySubscription = DateTime.Now;
                }

                var logEntries = consequences.ToLogEntries(_userContext, _operationClock);
                var addLogResult = organization.AddExternalImportLog(OrganizationUnitOrigin.STS_Organisation, logEntries);
                if (addLogResult.Failed)
                {
                    var error = addLogResult.Error;
                    _logger.Error("Failed adding change log while importing org tree for organization with uuid {uuid}. Failed with: {code}:{message}", command.Organization.Uuid, error.FailureType, error.Message);
                    transaction.Rollback();
                    return error;
                }

                var addNewLogsResult = addLogResult.Value;
                var removedChangeLogs = addNewLogsResult.RemovedChangeLogs.OfType<StsOrganizationChangeLog>().ToList();
                if (removedChangeLogs.Any())
                {
                    _stsChangeLogRepository.RemoveRange(removedChangeLogs);
                }
                
                _domainEvents.Raise(new EntityUpdatedEvent<Organization>(organization));
                _databaseControl.SaveChanges();
                transaction.Commit();
                return Maybe<OperationError>.None;

            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception during FK Org sync of organization with uuid:{uuid}", command.Organization.Uuid);
                transaction.Rollback();
                return new OperationError("Exception during import", OperationFailure.UnknownError);
            }
        }

        private bool IsBackgroundImport()
        {
            return _userContext.IsNone;
        }
    }
}
