using System.Collections.Generic;

namespace UI.MVC4.Models
{
    public class OrgUnitDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OrganizationId { get; set; }
        public int? ParentId { get; set; }
        public List<OrgUnitDTO> Children { get; set; }

        public long? Ean { get; set; }
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
}