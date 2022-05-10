using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.UIConfiguration
{
    public class UIModuleCustomization : Entity, IOwnedByOrganization
    {
        public int OrganizationId { get; set; }

        //TODO: Doesn not belong on the node level.. put it in a moduleconfig with moduleConfig--->*nodeConfig relation
        /// <summary>
        /// Application module, e.g. It-System
        /// </summary>
        public string Module { get; set; }
        
        public virtual Organization.Organization Organization{ get; set; }
        public virtual ICollection<CustomizedUINode> Nodes { get; set; }

    }
}
