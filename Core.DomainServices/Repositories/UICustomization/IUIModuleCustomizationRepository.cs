using Core.DomainModel.UIConfiguration;

namespace Core.DomainServices.Repositories.UICustomization
{
    public interface IUIModuleCustomizationRepository
    {
        void Update(DomainModel.Organization.Organization organization, UIModuleCustomization uiModuleCustomization);
    }
}
