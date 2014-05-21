using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public class AgreementElement : ICustomOptionEntity<ItContract>
    {
        public AgreementElement()
        {
            References = new List<ItContract>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItContract> References { get; set; }
        public int? CreatedByOrganizationId { get; set; }
        public Organization CreatedByOrganization { get; set; }
    }

    public interface ICustomOptionEntity<T> : IOptionEntity<T>
    {
        int? CreatedByOrganizationId { get; set; }
        Organization CreatedByOrganization { get; set; }
    }
}
