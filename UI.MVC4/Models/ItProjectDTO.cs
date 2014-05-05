using System.Collections.Generic;
using Core.DomainModel;

namespace UI.MVC4.Models
{
    public class ItProjectDTO
    {
        public int Id { get; set; }
        public string ItProjectId { get; set; }
        public string Background { get; set; }
        public bool IsTransversal { get; set; }
        public string Note { get; set; }
        public string Name { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public int? AssociatedProgramId { get; set; }
        public IEnumerable<int> AssociatedProjectIds { get; set; }
        public int ItProjectTypeId { get; set; }
        public int ItProjectCategoryId { get; set; }
        public int OrganizationId { get; set; }

        public IEnumerable<EconomyYearDTO> EconomyYears { get; set; } 
    }
}