using System;
using System.Collections.Generic;

namespace Presentation.Web.Models
{
    public class OrgUnitDTO
    {
        public int Id { get; set; }
        public string LocalId { get; set; }
        public string Name { get; set; }
        public int OrganizationId { get; set; }
        public int? ParentId { get; set; }
        public List<OrgUnitDTO> Children { get; set; }

        public long? Ean { get; set; }

        public string ObjectOwnerName { get; set; }
        public string ObjectOwnerLastName { get; set; }

        public string ObjectOwnerFullName
        {
            get { return ObjectOwnerName + " " + ObjectOwnerLastName; }
        }

        public DateTime LastChanged { get; set; }
        public int LastChangedByUserId { get; set; }
        public Guid Uuid { get; set; }
    }

    public class OrgUnitSimpleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        public string QualifiedName
        {
            get { return Name + ", " + OrganizationName; }
        }
    }

    public class SimpleOrgUnitDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
