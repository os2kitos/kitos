using Core.DomainModel.Organization;

namespace Core.DomainServices.Context
{
    public interface IDefaultOrganizationResolver
    {
        Organization Resolve();
    }
}
