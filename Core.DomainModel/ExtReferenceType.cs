using System.Collections.Generic;

namespace Core.DomainModel
{
    public class ExtReferenceType
    {
        public ExtReferenceType()
        {
            this.ExtReferences = new List<ExtReference>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<ExtReference> ExtReferences { get; set; }
    }
}
