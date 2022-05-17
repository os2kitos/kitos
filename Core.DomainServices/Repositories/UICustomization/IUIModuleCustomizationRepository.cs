using System.Collections.Generic;
using Core.DomainModel.UIConfiguration;

namespace Core.DomainServices.Repositories.UICustomization
{
    public interface IUIModuleCustomizationRepository
    {
        void DeleteNodes(IEnumerable<CustomizedUINode> nodes);
        void Update(UIModuleCustomization uiModuleCustomization);
    }
}
