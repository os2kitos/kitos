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
        Maybe<ItInterface> GetInterface(Guid uuid);
    }
}
