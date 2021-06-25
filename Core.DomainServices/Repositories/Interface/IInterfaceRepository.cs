using System;
using System.Linq;
using Core.DomainModel.ItSystem;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Interface
{
    public interface IInterfaceRepository
    {
        IQueryable<ItInterface> GetInterfacesWithExternalReferenceDefined();
        IQueryable<ItInterface> GetInterfaces();
        Maybe<ItInterface> GetInterface(int id);
        Maybe<ItInterface> GetInterface(Guid uuid);
        void Add(ItInterface newItInterface); 
        void Update(ItInterface itInterface);
        void Delete(ItInterface itInterface);
    }
}
