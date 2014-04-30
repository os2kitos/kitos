using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class ItSystemService : IItSystemService
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


        public IEnumerable<ItSystem> GetSystems(Organization organization, string nameSearch)
        {
            //TODO filter by organization or public

            if (nameSearch == null) return _repository.Get();

            return _repository.Get(system => system.Name.StartsWith(nameSearch));
        }

        public IEnumerable<ItSystem> GetNonInterfaces(Organization organization, string nameSearch)
        {
            return GetSystems(organization, nameSearch).Where(system => system.AppType.Id != InterfaceAppType.Id);
        }

        public IEnumerable<ItSystem> GetInterfaces(Organization organization, string nameSearch)
        {
            return GetSystems(organization, nameSearch).Where(system => system.AppType.Id == InterfaceAppType.Id);
        }
    }
}