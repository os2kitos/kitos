using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.DomainModel.ItSystem
{
    public class SensitivePersonalDataType : OptionEntity<ItSystem>, IOptionReference<ItSystem>
    {
        public virtual ICollection<ItSystem> References { get; set; } = new HashSet<ItSystem>();

        [NotMapped]
        public bool Checked { get; set; }
    }
}
