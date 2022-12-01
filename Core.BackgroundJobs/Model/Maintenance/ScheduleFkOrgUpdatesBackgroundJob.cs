using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.ScheduledJobs;
using Core.DomainModel.Commands;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Time;
using Serilog;

namespace Core.BackgroundJobs.Model.Maintenance
{
    public class ScheduleFkOrgUpdatesBackgroundJob : IAsyncBackgroundJob
    {
        private readonly IHangfireApi _hangfireApi;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOperationClock _operationClock;
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;
        public string Id => StandardJobIds.ScheduleFkOrgUpdates;

        public ScheduleFkOrgUpdatesBackgroundJob(
            IHangfireApi hangfireApi,
            IOrganizationRepository organizationRepository,
            ILogger logger,
            IOperationClock operationClock,
            ICommandBus commandBus)
        {
            _hangfireApi = hangfireApi;
            _organizationRepository = organizationRepository;
            _logger = logger;
            _operationClock = operationClock;
            _commandBus = commandBus;
        }

        public Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default)
        {
            var uuids = _organizationRepository
                .GetAll()
                .Where
                (x =>
                    x.StsOrganizationConnection != null &&
                    x.StsOrganizationConnection.Connected &&
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
                    var command = new AuthorizedUpdateOrganizationFromFKOrganisationCommand(organization, organization.StsOrganizationConnection.SynchronizationDepth.FromNullableValueType(), true);
                    var error = _commandBus.Execute<AuthorizedUpdateOrganizationFromFKOrganisationCommand, Maybe<OperationError>>(command);
                    if (error.HasValue)
                    {
                        _logger.Error("Error while automatically importing from FK org into org with uuid {uuid}. Error: {error}", organization, error.Select(e=>e.ToString()).GetValueOrDefault());
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Exception during FK Org sync of organization with uuid:{uuid}", organizationUuid);
                }
            }
        }
    }
}
