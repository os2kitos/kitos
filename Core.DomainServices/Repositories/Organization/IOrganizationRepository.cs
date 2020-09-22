using System.Linq;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Organization
{
    public interface IOrganizationRepository
    {
        IQueryable<DomainModel.Organization.Organization> GetAll();
        Maybe<DomainModel.Organization.Organization> GetById(int id);
        Maybe<DomainModel.Organization.Organization> GetByCvr(string cvrNumber);
    }
}
