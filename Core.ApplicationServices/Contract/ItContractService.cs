using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItContract;
using Core.DomainModel.Result;
using Core.DomainServices;

namespace Core.ApplicationServices.Contract
{
    public class ItContractService : IItContractService
    {
        private readonly IGenericRepository<EconomyStream> _economyStreamRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IGenericRepository<ItContract> _repository;

        public ItContractService(
            IGenericRepository<ItContract> repository, 
            IGenericRepository<EconomyStream> economyStreamRepository,
            IAuthorizationContext authorizationContext)
        {
            _repository = repository;
            _economyStreamRepository = economyStreamRepository;
            _authorizationContext = authorizationContext;
        }
        public Result<ItContract, OperationFailure> Delete(int id)
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

            // delete it interface
            _repository.DeleteWithReferencePreload(contract);
            _repository.Save();

            // delete orphan economy streams
            var orphanStreams = _economyStreamRepository.Get(x => x.InternPaymentForId == null && x.ExternPaymentForId == null);
            foreach (var orphan in orphanStreams)
            {
                _economyStreamRepository.DeleteByKeyWithReferencePreload(orphan.Id);
            }
            _economyStreamRepository.Save();

            return contract;
        }
    }
}
