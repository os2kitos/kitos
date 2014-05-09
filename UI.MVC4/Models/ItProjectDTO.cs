using System.Collections.Generic;
using Core.DomainModel;

namespace UI.MVC4.Models
{
    public class ItProjectDTO
    {
        public int Id { get; set; }
        public string ObjectOwnerName { get; set; }
        public string ItProjectId { get; set; }
        public string Background { get; set; }
        public bool IsTransversal { get; set; }
        public bool IsStrategy { get; set; }
        public string Note { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AccessModifier AccessModifier { get; set; }

        public int? AssociatedProgramId { get; set; }
        public string AssociatedProgramName { get; set; }

        public IEnumerable<int> AssociatedProjectIds { get; set; }

        public int ItProjectTypeId { get; set; }
        public string ItProjectTypeName { get; set; }
        public int ItProjectCategoryId { get; set; }
        public string ItProjectCategoryName { get; set; }
        public int OrganizationId { get; set; }
        public IEnumerable<OrgUnitDTO> UsedByOrgUnits { get; set; }
        public IEnumerable<EconomyYearDTO> EconomyYears { get; set; } 
        public IEnumerable<ItSystemDTO> ItSystems { get; set; }
        public IEnumerable<TaskRefDTO> TaskRefs { get; set; }

        public int? JointMunicipalProjectId { get; set; }
        public int? CommonPublicProjectId { get; set; }

        public int? ResponsibleOrgUnitId { get; set; }
        public OrgUnitDTO ResponsibleOrgUnit { get; set; }

        public int? ParentItProjectId { get; set; }
        public ItProjectDTO ParentItProject { get; set; }
    }
}