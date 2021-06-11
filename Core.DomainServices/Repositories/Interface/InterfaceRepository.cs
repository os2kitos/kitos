using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Extensions;

namespace Core.DomainServices.Repositories.Interface
{
    public class InterfaceRepository : IInterfaceRepository
    {
        private readonly IGenericRepository<ItInterface> _interfaceRepository;

        public InterfaceRepository(IGenericRepository<ItInterface> interfaceRepository)
        {
            _interfaceRepository = interfaceRepository;
        }

        public IQueryable<ItInterface> GetInterfacesFromOrganization(int orgId)
        {
            return _interfaceRepository.AsQueryable().ByOrganizationId(orgId);
        }

        public IQueryable<ItInterface> GetInterfacesWithExternalReferenceDefined()
        {
            return _interfaceRepository.AsQueryable().Where(x => x.Url != null && x.Url != "");
        }
    }
}
