using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.DomainModel
{
    public abstract class OptionHasChecked<T> : OptionEntity<T>, IOptionReference<T>
    {
        public virtual ICollection<T> References { get; set; } = new HashSet<T>();

        [NotMapped]
        public bool Checked { get; set; }
    }
}
