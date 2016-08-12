using System.Collections.Generic;

namespace Core.DomainModel.Reports
{
    public class ReportCategoryType : Entity, IOptionEntity<Report>
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public ICollection<Report> References { get; set; }
    }
}