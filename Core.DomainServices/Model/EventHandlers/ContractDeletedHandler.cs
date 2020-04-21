using System;
using System.Data;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Serilog;

namespace Core.DomainServices.Model.EventHandlers
{
    public class ContractDeletedHandler : IDomainEventHandler<ContractDeleted>
    {
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;

        public ContractDeletedHandler(
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            ITransactionManager transactionManager,
            ILogger logger
            )
        {
            _systemUsageRepository = systemUsageRepository;
            _transactionManager = transactionManager;
            _logger = logger;
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

                var systemRelations = deletedContract.AssociatedSystemRelations.ToList();
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
                            relationInterface: systemRelation.RelationInterface,
                            toContract: Maybe<ItContract>.None,  //Reset the contract
                            toFrequency: systemRelation.UsageFrequency);

                        if (result.Failed)
                            throw new InvalidOperationException($"Failed to modify system relation. Error: {result.Error}");
                    }

                    _systemUsageRepository.Save();
                    transaction.Commit();
                }
            }
        }
    }
}
