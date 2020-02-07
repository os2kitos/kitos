using System;
using System.Data;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices.Context;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Serilog;

namespace Core.DomainServices.Model.EventHandlers
{
    public class ContractDeletedHandler : IDomainEventHandler<ContractDeleted>
    {
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IOperationClock _clock;
        private readonly ILogger _logger;
        private readonly Maybe<ActiveUserContext> _userContext;

        public ContractDeletedHandler(
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            ITransactionManager transactionManager,
            Maybe<ActiveUserContext> userContext,
            IOperationClock clock,
            ILogger logger
            )
        {
            _systemUsageRepository = systemUsageRepository;
            _transactionManager = transactionManager;
            _clock = clock;
            _logger = logger;
            _userContext = userContext;
        }

        public void Handle(ContractDeleted domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                var deletedContract = domainEvent.DeletedContract;

                _logger.Debug(
                    "Contract with id {contractId} deleted. Resetting 'contract' field on all associated system relations",
                    deletedContract.Id);

                var updateTime = _clock.Now;
                var systemRelations = deletedContract.AssociatedSystemRelations.ToList();
                if (systemRelations.Any())
                {
                    foreach (var systemRelation in systemRelations)
                    {
                        var activeUser = _userContext.Match(ctx => ctx.UserEntity, () => systemRelation.ObjectOwner);

                        var fromSystemUsage = systemRelation.FromSystemUsage;

                        var result = fromSystemUsage.ModifyUsageRelation(
                            activeUser: activeUser,
                            relationId: systemRelation.Id,
                            toSystemUsage: systemRelation.ToSystemUsage,
                            changedDescription: systemRelation.Description,
                            changedReference: systemRelation.Reference,
                            relationInterface: systemRelation.RelationInterface,
                            toContract: Maybe<ItContract>.None,  //Reset the contract
                            toFrequency: systemRelation.UsageFrequency);

                        if (result.Failed)
                            throw new InvalidOperationException($"Failed to modify system relation. Error: {result.Error}");

                        fromSystemUsage.LastChangedByUser = activeUser;
                        fromSystemUsage.LastChanged = updateTime;
                    }

                    _systemUsageRepository.Save();
                    transaction.Commit();
                }
            }
        }
    }
}
