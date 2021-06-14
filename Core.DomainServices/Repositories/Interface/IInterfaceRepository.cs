using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystem;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Interface
{
    public interface IInterfaceRepository
    {
        IQueryable<ItInterface> GetInterfacesWithExternalReferenceDefined();
        IQueryable<ItInterface> GetInterfaces();
        IQueryable<ItInterface> GetInterfacesFromRightsHolderOrganizations(IEnumerable<int> rightsHolderOrgs);
        Maybe<ItInterface> GetInterface(Guid uuid);
    }
}
