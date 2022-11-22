using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.ScheduledJobs;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Organizations;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.BackgroundJobs.Model.Maintenance
{
    public class ScheduleFkOrgUpdatesBackgroundJob : IAsyncBackgroundJob
    {
        private readonly IHangfireApi _hangfireApi;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IStsOrganizationUnitService _stsOrganizationUnitService;
        private readonly ITransactionManager _transactionManager;
        private readonly IDatabaseControl _databaseControl;
        private readonly IGenericRepository<OrganizationUnit> _organizationUnitRepository;
        private readonly IDomainEvents _domainEvents;
        private readonly IOperationClock _operationClock;
        private readonly ILogger _logger;
        public string Id => StandardJobIds.ScheduleFkOrgUpdates;

        public ScheduleFkOrgUpdatesBackgroundJob(
            IHangfireApi hangfireApi,
            IOrganizationRepository organizationRepository,
            ILogger logger,
            IStsOrganizationUnitService stsOrganizationUnitService,
            ITransactionManager transactionManager,
            IDatabaseControl databaseControl,
            IGenericRepository<OrganizationUnit> organizationUnitRepository,
            IDomainEvents domainEvents,
            IOperationClock operationClock)
        {
            _hangfireApi = hangfireApi;
            _organizationRepository = organizationRepository;
            _logger = logger;
            _stsOrganizationUnitService = stsOrganizationUnitService;
            _transactionManager = transactionManager;
            _databaseControl = databaseControl;
            _organizationUnitRepository = organizationUnitRepository;
            _domainEvents = domainEvents;
            _operationClock = operationClock;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var uuids = _organizationRepository
                .GetAll()
                .Where
                (x =>
                    x.StsOrganizationConnection != null &&
                    x.StsOrganizationConnection.SubscribeToUpdates &&
                    x.StsOrganizationConnection.SubscribeToUpdates
                )
                .Select(x => x.Uuid)
                .ToList();

            var counter = 0;
            var offsetInMinutes = 0;
            var dateTimeReference = _operationClock.Now;
            foreach (var uuid in uuids)
            {
                //Add some spread to the start of the synchronizations
                offsetInMinutes += (counter % 2); // allow two in parallel
                var offset = dateTimeReference.AddMinutes(offsetInMinutes);
                _hangfireApi.Schedule(() => PerformImportFromFKOrganisation(uuid), offset);
                counter++;
            }

            return Task.FromResult(Result<string, OperationError>.Success($"Scheduled {uuids.Count} sync jobs"));
        }

        public void PerformImportFromFKOrganisation(Guid organizationUuid)
        {
            //TODO: Make a generic domain command which can deal with updating the different tables and so on. Event for the log entries
            //TODO: Thereby the application services can deal with user access auth and the call the command which can also be called by this one
            using var transaction = _transactionManager.Begin();
            var getOrganizationResult = _organizationRepository.GetByUuid(organizationUuid);
            if (getOrganizationResult.IsNone)
            {
                _logger.Error("Unable to perform sync for organization with uui {uuid} because the repository returned None", organizationUuid);
            }
            else
            {
                var organization = getOrganizationResult.Value;
                if (organization.StsOrganizationConnection?.SubscribeToUpdates != true)
                {
                    _logger.Warning("Sync job for organization with uuid {uuid} ignored since organization no longer subscribes to updates", organizationUuid);
                    return;
                }
                try
                {
                    var organizationTree = _stsOrganizationUnitService.ResolveOrganizationTree(organization);
                    if (organizationTree.Failed)
                    {
                        var error = organizationTree.Error;
                        _logger.Error("Unable to resolve external org tree for organization with uuid {uuid}. Failed with: {code}:{detail}:{message}", organizationUuid, error.FailureType, error.Detail, error.Message);
                        return;
                    }

                    var updateResult = organization.UpdateConnectionToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, organizationTree.Value, organization.StsOrganizationConnection.SynchronizationDepth.FromNullableValueType(), true);
                    if (updateResult.Failed)
                    {
                        var error = updateResult.Error;
                        _logger.Error("Failed importing org tree for organization with uuid {uuid}. Failed with: {code}:{message}", organizationUuid, error.FailureType, error.Message);
                        transaction.Rollback();
                        return;
                    }

                    var consequences = updateResult.Value;

                    if (consequences.DeletedExternalUnitsBeingDeleted.Any())
                    {
                        _organizationUnitRepository.RemoveRange(consequences.DeletedExternalUnitsBeingDeleted);
                    }
                    foreach (var (affectedUnit, _, _) in consequences.OrganizationUnitsBeingRenamed)
                    {
                        _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(affectedUnit));
                    }
                    organization.StsOrganizationConnection.DateOfLatestCheckBySubscription = DateTime.Now;
                    //TODO: Add entry to the change log - only if there are any consequences - otherwise ignore it!

                    _databaseControl.SaveChanges();
                    transaction.Commit();

                }
                catch (Exception e)
                {
                    _logger.Error(e, "Exception during FK Org sync of organization with uuid:{uuid}", organizationUuid);
                }
            }
        }
    }
}
