using System;
using Core.DomainModel;

namespace Presentation.Web.Models
{
    public class OrganizationSimpleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ConfigDTO Config { get; set; }

        public OrgUnitSimpleDTO Root { get; set; }
    }
}
