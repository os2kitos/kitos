using Core.DomainModel.Result;

namespace Core.DomainServices.Repositories.Organization
{
    public interface IOrganizationRepository
    {
        Maybe<DomainModel.Organization.Organization> GetByCvr(string cvrNumber);
    }
}
