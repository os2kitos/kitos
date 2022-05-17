using Core.Abstractions.Types;
using Core.ApplicationServices.Model.UiCustomization;
using Core.DomainModel.UIConfiguration;

namespace Core.ApplicationServices.UIConfiguration
{
    public interface IUIModuleCustomizationService
    {
        Result<UIModuleCustomization, OperationError> GetModuleConfigurationForOrganization(int organizationId, string module);
        Maybe<OperationError> UpdateModule(UIModuleCustomizationParameters parameters);
    }
}
