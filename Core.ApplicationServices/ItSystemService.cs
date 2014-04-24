using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices
{
    class ItSystemService : IItSystemService
    {
        private readonly IGenericRepository<ItSystem> _repository;
        private readonly IGenericRepository<AppType> _appTypeRepository;

        public ItSystemService(IGenericRepository<ItSystem> repository, IGenericRepository<AppType> appTypeRepository)
        {
            _repository = repository;
            _appTypeRepository = appTypeRepository;
            InterfaceAppType = _appTypeRepository.Get(appType => appType.Name == "Snitflade").Single();
        }

        public AppType InterfaceAppType { get; private set; }


        public IEnumerable<ItSystem> GetAllSystems(Organization organization)
        {
            //TODO filter by organization or public
            return _repository.Get();
        }

        public IEnumerable<ItSystem> GetAllNonInterfaces(Organization organization)
        {
            return GetAllSystems(organization).Where(system => system.AppType.Id != InterfaceAppType.Id);
        }

        public IEnumerable<ItSystem> GetAllInterfaces(Organization organization)
        {
            return GetAllSystems(organization).Where(system => system.AppType.Id == InterfaceAppType.Id);
        }
    }
}