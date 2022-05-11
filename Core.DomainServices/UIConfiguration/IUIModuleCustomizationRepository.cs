using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainModel.UIConfiguration;

namespace Core.DomainServices.UIConfiguration
{
    public interface IUIModuleCustomizationRepository
    {
        void Update(Organization organization);
        IQueryable<UIModuleCustomization> GetModuleConfigurationForOrganization(int organizationId, string module);
    }
}
