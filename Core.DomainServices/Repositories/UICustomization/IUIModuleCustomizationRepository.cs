using System.Collections.Generic;
using Core.DomainModel.UIConfiguration;
using Core.Abstractions.Types;

namespace Core.DomainServices.Repositories.UICustomization
{
    public interface IUIModuleCustomizationRepository
    {
        Maybe<UIModuleCustomization> GetByOrganizationAndModule(int organizationId, string module);
        void DeleteNodes(IEnumerable<CustomizedUINode> nodes);
        void Update(UIModuleCustomization uiModuleCustomization);
    }
}
