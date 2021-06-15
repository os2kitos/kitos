using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Interface
{
    public class InterfaceRepository : IInterfaceRepository
    {
        private readonly IGenericRepository<ItInterface> _interfaceRepository;

        public InterfaceRepository(IGenericRepository<ItInterface> interfaceRepository)
        {
            _interfaceRepository = interfaceRepository;
        }

        public Maybe<ItInterface> GetInterface(Guid uuid)
        {
            return _interfaceRepository.AsQueryable().ByUuid(uuid);
        }

        public IQueryable<ItInterface> GetInterfaces()
        {
            return _interfaceRepository.AsQueryable();
        }

        public IQueryable<ItInterface> GetInterfacesWhereRightsHolderIsOneOf(IEnumerable<int> rightsHolderOrgs)
        {
            if(rightsHolderOrgs == null)
            {
                throw new ArgumentNullException(nameof(rightsHolderOrgs));
            }
            return _interfaceRepository.AsQueryable().ByRightsHolderIds(rightsHolderOrgs);
        }

        public IQueryable<ItInterface> GetInterfacesWithExternalReferenceDefined()
        {
            return _interfaceRepository.AsQueryable().Where(x => x.Url != null && x.Url != "");
        }
    }
}
