using System.Linq;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices.Repositories.Interface
{
    public interface IInterfaceRepository
    {
        IQueryable<ItInterface> GetInterfacesWithExternalReferenceDefined();
    }
}
