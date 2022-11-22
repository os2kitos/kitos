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
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.BackgroundJobs.Model.ReadModels
{
    public class ScheduleFkOrgUpdatesBackgroundJob : IAsyncBackgroundJob
    {
        private readonly IHangfireApi _hangfireApi;
        private readonly IGenericRepository<StsOrganizationConnection> _connectionRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IStsOrganizationUnitService _stsOrganizationUnitService;
        private readonly ITransactionManager _transactionManager;
        private readonly IDatabaseControl _databaseControl;
        private readonly IGenericRepository<OrganizationUnit> _organizationUnitRepository;
        private readonly IDomainEvents _domainEvents;
        private readonly ILogger _logger;
        public string Id => StandardJobIds.ScheduleFkOrgUpdates;

        public ScheduleFkOrgUpdatesBackgroundJob(
            IHangfireApi hangfireApi,
            IGenericRepository<StsOrganizationConnection> connectionRepository,
            IOrganizationRepository organizationRepository,
            ILogger logger,
            IStsOrganizationUnitService stsOrganizationUnitService,
            ITransactionManager transactionManager,
            IDatabaseControl databaseControl,
            IGenericRepository<OrganizationUnit> organizationUnitRepository,
            IDomainEvents domainEvents)
        {
            _hangfireApi = hangfireApi;
            _connectionRepository = connectionRepository;
            _organizationRepository = organizationRepository;
            _logger = logger;
            _stsOrganizationUnitService = stsOrganizationUnitService;
            _transactionManager = transactionManager;
            _databaseControl = databaseControl;
            _organizationUnitRepository = organizationUnitRepository;
            _domainEvents = domainEvents;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var uuids = _connectionRepository
                .AsQueryable()
                .Where(x => x.Connected && x.SubscribeToUpdates)
                .Select(x => x.Organization.Uuid)
                .ToList();

            var startOffsetInMinutes = 0;
            var dateTimeReference = DateTimeOffset.Now;
            foreach (var uuid in uuids)
            {
                //Add some spread to the start of the synchronizations
                var offset = dateTimeReference.AddMinutes(startOffsetInMinutes); //TODO: Consider the interval - how many can start at the time
                _hangfireApi.Schedule(() => PerformImportFromFKOrganisation(uuid), offset);
                startOffsetInMinutes++;
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
                    _logger.Warning("Sync job for organization with uuid {uuid} ignored since organization no longer subscribes to updates");
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
