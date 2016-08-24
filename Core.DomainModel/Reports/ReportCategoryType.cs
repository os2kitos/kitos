using System.Collections.Generic;

namespace Core.DomainModel.Reports
{
    public class ReportCategoryType : Entity, IOptionEntity<Report>
    {
        public ReportCategoryType()
        {
            References = new List<Report>();
        }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<Report> References { get; set; }
    }
}