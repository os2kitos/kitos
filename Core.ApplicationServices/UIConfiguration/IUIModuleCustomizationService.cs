using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.UiCustomization;
using Core.DomainModel.UIConfiguration;

namespace Core.ApplicationServices.UIConfiguration
{
    public interface IUIModuleCustomizationService
    {
        Result<UIModuleCustomization, OperationError> UpdateModuleAndGet(UIModuleCustomizationParameters parameters);
        Result<UIModuleCustomization, OperationError> GetModuleCustomizationByOrganizationUuid(Guid organizationUuid, string module);
    }
}
