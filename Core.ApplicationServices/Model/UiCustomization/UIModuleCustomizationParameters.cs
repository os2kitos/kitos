using System.Collections.Generic;

namespace Core.ApplicationServices.Model.UiCustomization
{
    public class UIModuleCustomizationParameters
    {
        public int OrganizationId { get; set; }
        public string Module { get; set; }

        public IEnumerable<CustomUINodeParameters> Nodes{ get; set; }

        public UIModuleCustomizationParameters(int organizationId, string module, IEnumerable<CustomUINodeParameters> nodes)
        {
            OrganizationId = organizationId;
            Module = module;
            Nodes = nodes;
        }
    }
}
