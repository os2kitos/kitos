using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.Result;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;

namespace Core.ApplicationServices.Contract
{
    public class ItContractService : IItContractService
    {
        private readonly IGenericRepository<EconomyStream> _economyStreamRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IGenericRepository<ItContract> _repository;

        public ItContractService(
            IGenericRepository<ItContract> repository,
            IGenericRepository<EconomyStream> economyStreamRepository,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IAuthorizationContext authorizationContext)
        {
            _repository = repository;
            _economyStreamRepository = economyStreamRepository;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _authorizationContext = authorizationContext;
        }
        public Result<ItContract, OperationFailure> Delete(int id)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                var contract = _repository.GetByKey(id);

                if (contract == null)
                {
                    return OperationFailure.NotFound;
                }

                if (!_authorizationContext.AllowDelete(contract))
                {
                    return OperationFailure.Forbidden;
                }

                //Delete the economy streams to prevent them from being orphaned
                foreach (var economyStream in GetEconomyStreams(contract))
                {
                    DeleteEconomyStream(economyStream);
                }
                _economyStreamRepository.Save();

                //Delete the contract
                _domainEvents.Raise(new ContractDeleted(contract));
                _repository.DeleteWithReferencePreload(contract);
                _repository.Save();

                transaction.Commit();

                return contract;
            }
        }

        private static IEnumerable<EconomyStream> GetEconomyStreams(ItContract contract)
        {
            return contract
                .ExternEconomyStreams
                .ToList()
                .Concat(contract.InternEconomyStreams.ToList());
        }

        private void DeleteEconomyStream(EconomyStream economyStream)
        {
            _economyStreamRepository.DeleteWithReferencePreload(economyStream);
        }
    }
}
