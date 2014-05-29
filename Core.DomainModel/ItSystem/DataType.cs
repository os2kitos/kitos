using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core.DomainModel.ItSystem
{
    public class DataType : Entity, IOptionEntity<DataRow>
    {
        public DataType()
        {
            References = new List<DataRow>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<DataRow> References { get; set; }
    }
}