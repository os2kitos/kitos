using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItContract
{
    public class AgreementElement : IOptionEntity<Agreement> //TODO: References to Agreement or ItContract?
    {
        public AgreementElement()
        {
            References = new List<Agreement>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<Agreement> References { get; set; }
    }
}
