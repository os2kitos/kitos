using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;

namespace Core.DomainModel.UIConfiguration
{
    public class UIModuleCustomization : Entity, IOwnedByOrganization
    {
        public int OrganizationId { get; set; }
        
        /// <summary>
        /// Application module, e.g. It-System
        /// </summary>
        public string Module { get; set; }
        
        public virtual Organization.Organization Organization{ get; set; }
        public virtual ICollection<CustomizedUINode> Nodes { get; set; }

        public void MirrorNodes(ICollection<CustomizedUINode> nodes)
        {
            Nodes ??= new List<CustomizedUINode>();

            nodes.MirrorTo(Nodes, x => x.Key);
        }
    }
}
