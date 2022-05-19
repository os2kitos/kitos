using System.Collections.Generic;

namespace Core.ApplicationServices.Model.UiCustomization
{
    public class UIModuleCustomizationParameters
    {
        public int OrganizationId { get; }
        public string Module { get; }

        public IEnumerable<CustomUINodeParameters> Nodes { get; }

        public UIModuleCustomizationParameters(int organizationId, string module, IEnumerable<CustomUINodeParameters> nodes)
        {
            OrganizationId = organizationId;
            Module = module;
            Nodes = nodes;
        }
    }
}
