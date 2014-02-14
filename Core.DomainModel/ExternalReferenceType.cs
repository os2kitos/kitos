using System.Collections.Generic;

namespace Core.DomainModel
{
    public class ExternalReferenceType
    {
        public ExternalReferenceType()
        {
            this.ExternalReferences = new List<ExternalReference>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<ExternalReference> ExternalReferences { get; set; }
    }
}
