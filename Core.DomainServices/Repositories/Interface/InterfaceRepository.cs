using System;
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

        public IQueryable<ItInterface> GetInterfacesWithExternalReferenceDefined()
        {
            return _interfaceRepository.AsQueryable().Where(x => x.Url != null && x.Url != "");
        }

        public void Add(ItInterface newItInterface)
        {
            if (newItInterface == null)
                throw new ArgumentNullException(nameof(newItInterface));

            _interfaceRepository.Insert(newItInterface);
            _interfaceRepository.Save();
        }

        public void Update(ItInterface itInterface)
        {
            if (itInterface == null)
                throw new ArgumentNullException(nameof(itInterface));

            _interfaceRepository.Update(itInterface);
            _interfaceRepository.Save();
        }

        public Maybe<ItInterface> GetInterface(int id)
        {
            return _interfaceRepository.AsQueryable().ById(id);
        }

        public void Delete(ItInterface itInterface)
        {
            if (itInterface == null)
                throw new ArgumentNullException(nameof(itInterface));

            _interfaceRepository.DeleteWithReferencePreload(itInterface);
            _interfaceRepository.Save();
        }
    }
}
