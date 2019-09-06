using Core.DomainModel.ItContract;
using Core.DomainServices;
using System.Linq;

namespace Core.ApplicationServices
{
    public class ItContractService : IItContractService
    {
        private readonly IGenericRepository<EconomyStream> _economyStreamRepository;
        private readonly IGenericRepository<ItContract> _repository;

        public ItContractService(IGenericRepository<ItContract> repository, IGenericRepository<EconomyStream> economyStreamRepository)
        {
            _repository = repository;
            _economyStreamRepository = economyStreamRepository;
        }
        public void Delete(int id)
        {
            // http://stackoverflow.com/questions/15226312/entityframewok-how-to-configure-cascade-delete-to-nullify-foreign-keys
            // when children are loaded into memory the foreign key is correctly set to null on children when deleted
            var contract = _repository.Get(x => x.Id == id, null, $"{nameof(ItContract.AssociatedAgreementElementTypes)}, {nameof(ItContract.ExternEconomyStreams)}, {nameof(ItContract.InternEconomyStreams)}, {nameof(ItContract.AssociatedInterfaceExposures)}").FirstOrDefault();

            // delete it interface
            _repository.Delete(contract);
            _repository.Save();

            // delete orphan economy streams
            var orphanStreams = _economyStreamRepository.Get(x => x.InternPaymentForId == null && x.ExternPaymentForId == null);
            foreach (var orphan in orphanStreams)
            {
                _economyStreamRepository.DeleteByKey(orphan.Id);
            }
            _economyStreamRepository.Save();
        }
    }
}
