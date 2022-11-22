using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Commands;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using System;
using System.Linq;
using Core.DomainServices;
using Core.DomainServices.Organizations;
using Infrastructure.Services.DataAccess;
using Serilog;

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

        public AuthorizedUpdateOrganizationFromFKOrganisationCommandHandler(
            IStsOrganizationUnitService stsOrganizationUnitService,
            IGenericRepository<OrganizationUnit> organizationUnitRepository,
            ILogger logger,
            IDomainEvents domainEvents,
            IDatabaseControl databaseControl,
            ITransactionManager transactionManager)
        {
            _stsOrganizationUnitService = stsOrganizationUnitService;
            _organizationUnitRepository = organizationUnitRepository;
            _logger = logger;
            _domainEvents = domainEvents;
            _databaseControl = databaseControl;
            _transactionManager = transactionManager;
        }

        public Maybe<OperationError> Execute(AuthorizedUpdateOrganizationFromFKOrganisationCommand command)
        {
            var organization = command.Organization;
            using var transaction = _transactionManager.Begin();
            try
            {
                var organizationTree = _stsOrganizationUnitService.ResolveOrganizationTree(organization);
                if (organizationTree.Failed)
                {
                    var error = organizationTree.Error;
                    _logger.Error("Unable to resolve external org tree for organization with uuid {uuid}. Failed with: {code}:{detail}:{message}", command.Organization.Uuid, error.FailureType, error.Detail, error.Message);
                    return new OperationError($"Failed to resolve org tree:{error.Message.GetValueOrFallback("")}:{error.Detail:G}:{error.FailureType:G}", error.FailureType);
                }

                var updateResult = organization.UpdateConnectionToExternalOrganizationHierarchy(OrganizationUnitOrigin.STS_Organisation, organizationTree.Value, command.SynchronizationDepth, command.SubscribeToChanges);
                if (updateResult.Failed)
                {
                    var error = updateResult.Error;
                    _logger.Error("Failed importing org tree for organization with uuid {uuid}. Failed with: {code}:{message}", command.Organization.Uuid, error.FailureType, error.Message);
                    transaction.Rollback();
                    return new OperationError($"Failed to import org tree:{error.Message.GetValueOrFallback("")}:{error.FailureType:G}", error.FailureType);
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
                return Maybe<OperationError>.None;

            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception during FK Org sync of organization with uuid:{uuid}", command.Organization.Uuid);
                transaction.Rollback();
                return new OperationError("Exception during import", OperationFailure.UnknownError);
            }
        }
    }
}
