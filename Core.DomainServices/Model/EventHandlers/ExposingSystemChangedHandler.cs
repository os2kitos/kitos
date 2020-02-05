using System;
using System.Data;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices.Context;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Serilog;

namespace Core.DomainServices.Model.EventHandlers
{
    public class ExposingSystemChangedHandler : IDomainEventHandler<ExposingSystemChanged>
    {
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IOperationClock _clock;
        private readonly ILogger _logger;
        private readonly Maybe<ActiveUserContext> _userContext;

        public ExposingSystemChangedHandler(
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            ITransactionManager transactionManager,
            Maybe<ActiveUserContext> userContext,
            IOperationClock clock,
            ILogger logger)
        {
            _systemUsageRepository = systemUsageRepository;
            _transactionManager = transactionManager;
            _userContext = userContext;
            _clock = clock;
            _logger = logger;
        }

        public void Handle(ExposingSystemChanged domainEvent)
        {
            if (domainEvent == null)
                throw new ArgumentNullException(nameof(domainEvent));

            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                _logger.Debug(
                    "Exposing system for interface with id {interfaceId} changed from {fromSystemId} to {toSystemId}. Resetting 'interface' field on all associated system relations",
                    domainEvent.AffectedInterface.Id,
                    domainEvent.PreviousSystem.Match(x => x.Id.ToString(), () => "_none_"),
                    domainEvent.NewSystem.Match(x => x.Id.ToString(), () => "_none_"));

                var updateTime = _clock.Now;

                foreach (var systemRelation in domainEvent.AffectedInterface.AssociatedSystemRelations.ToList())
                {
                    var activeUser = _userContext.Match(ctx => ctx.UserEntity, () => systemRelation.ObjectOwner);

                    var fromSystemUsage = systemRelation.FromSystemUsage;

                    fromSystemUsage.ModifyUsageRelation(
                        activeUser: activeUser,
                        relationId: systemRelation.Id,
                        toSystemUsage: systemRelation.ToSystemUsage,
                        changedDescription: systemRelation.Description,
                        changedReference: systemRelation.Reference,
                        relationInterface: Maybe<ItInterface>.None, //Remove the interface binding
                        toContract: systemRelation.AssociatedContract,
                        toFrequency: systemRelation.UsageFrequency);
                    fromSystemUsage.LastChangedByUser = activeUser;
                    fromSystemUsage.LastChanged = updateTime;
                }

                _systemUsageRepository.Save();

                transaction.Commit();
            }
        }
    }
}
