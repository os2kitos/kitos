using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.UIConfiguration
{
    public class UIVisibilityConfiguration : Entity, IOwnedByOrganization
    {
        public int OrganizationId { get; set; }

        /// <summary>
        /// Application module, e.g. It-System
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// Contains a route to the property, for which visibility is being set
        /// e.g. module.group.field
        /// </summary>
        public string Key { get; set; }
        public string Enabled { get; set; }
        
        public Organization.Organization Organization{ get; set; }

    }
}
