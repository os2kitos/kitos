using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainModel.UIConfiguration;

namespace Core.ApplicationServices.UIConfiguration
{
    public interface IUIModuleCustomizationService
    {
        Result<List<UIModuleCustomization>, OperationError> GetModuleConfigurationForOrganization(int organizationId, string module);
        Result<List<UIModuleCustomization>, OperationError> Put(int organizationId, string module, UIModuleCustomization configuration);
    }
}
