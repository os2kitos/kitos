using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class DatabaseType : IEntity<int>
    {
        public DatabaseType()
        {
            this.Technologies = new List<Technology>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }

        public virtual ICollection<Technology> Technologies { get; set; }
    }
}
