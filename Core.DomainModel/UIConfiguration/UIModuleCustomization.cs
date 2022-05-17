using System.Collections.Generic;
using Core.Abstractions.Extensions;

namespace Core.DomainModel.UIConfiguration
{
    public class UIModuleCustomization : Entity, IOwnedByOrganization
    {
        public UIModuleCustomization()
        {
            Nodes = new List<CustomizedUINode>();
        }
        public int OrganizationId { get; set; }
        
        /// <summary>
        /// Application module, e.g. It-System
        /// </summary>
        public string Module { get; set; }
        
        public virtual Organization.Organization Organization{ get; set; }
        public virtual ICollection<CustomizedUINode> Nodes { get; set; }

        public void UpdateConfigurationNodes(IEnumerable<CustomizedUINode> nodes)
        {
            nodes.MirrorTo(Nodes, x => x.Key);
        }
    }
}
