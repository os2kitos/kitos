using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public partial class ProgLanguage
    {
        public ProgLanguage()
        {
            this.Technologies = new List<Technology>();
        }

        public int Id { get; set; }
        public virtual ICollection<Technology> Technologies { get; set; }
    }
}
