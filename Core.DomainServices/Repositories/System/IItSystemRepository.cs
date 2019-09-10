using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Model;

namespace Core.DomainServices.Repositories.System
{
    public interface IItSystemRepository
    {
        IQueryable<ItSystem> GetUnusedSystems(OrganizationDataQueryParameters parameters);

        IQueryable<ItSystem> GetSystemsInUse(int organizationId);
    }
}
