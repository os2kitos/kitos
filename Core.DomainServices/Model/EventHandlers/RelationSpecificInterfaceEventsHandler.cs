using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.ItSystemUsage;
using Infrastructure.Services.DataAccess;

using Serilog;

namespace Core.DomainServices.Model.EventHandlers
{
    public class RelationSpecificInterfaceEventsHandler :
        IDomainEventHandler<ExposingSystemChanged>,
        IDomainEventHandler<EntityBeingDeletedEvent<ItInterface>>
    {
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;

        public RelationSpecificInterfaceEventsHandler(
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            ITransactionManager transactionManager,
            ILogger logger)
        {
            _systemUsageRepository = systemUsageRepository;
            _transactionManager = transactionManager;
            _logger = logger;
        }

        public void Handle(ExposingSystemChanged domainEvent)
        {
            if (domainEvent == null)
                throw new ArgumentNullException(nameof(domainEvent));

            using (var transaction = _transactionManager.Begin())
            {
                var affectedInterface = domainEvent.AffectedInterface;

                _logger.Debug(
                    "Exposing system for interface with id {interfaceId} changed from {fromSystemId} to {toSystemId}. Resetting 'interface' field on all associated system relations",
                    affectedInterface.Id,
                    domainEvent.PreviousSystem.Match(x => x.Id.ToString(), () => "_none_"),
                    domainEvent.NewSystem.Match(x => x.Id.ToString(), () => "_none_"));

                ResetInterfaceOnRelations(affectedInterface, transaction);
            }
        }

        private void ResetInterfaceOnRelations(ItInterface affectedInterface, IDatabaseTransaction transaction)
        {
            var systemRelations = affectedInterface.AssociatedSystemRelations.ToList();
            if (systemRelations.Any())
            {
                foreach (var systemRelation in systemRelations)
                {
                    var fromSystemUsage = systemRelation.FromSystemUsage;

                    var result = fromSystemUsage.ModifyUsageRelation(
                        relationId: systemRelation.Id,
                        toSystemUsage: systemRelation.ToSystemUsage,
                        changedDescription: systemRelation.Description,
                        changedReference: systemRelation.Reference,
                        relationInterface: Maybe<ItInterface>.None, //Remove the interface binding
                        toContract: systemRelation.AssociatedContract,
                        toFrequency: systemRelation.UsageFrequency);

                    if (result.Failed)
                        throw new InvalidOperationException($"Failed to modify system relation. Error: {result.Error}");
                }

                _systemUsageRepository.Save();
                transaction.Commit();
            }
        }

        public void Handle(EntityBeingDeletedEvent<ItInterface> domainEvent)
        {
            if (domainEvent == null)
                throw new ArgumentNullException(nameof(domainEvent));

            using var transaction = _transactionManager.Begin();
            var affectedInterface = domainEvent.Entity;

            _logger.Debug(
                "Interface with id {interfaceId} deleted. Resetting 'interface' field on all associated system relations",
                affectedInterface.Id);

            ResetInterfaceOnRelations(affectedInterface, transaction);
        }
    }
}
