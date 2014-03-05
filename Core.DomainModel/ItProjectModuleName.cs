using System.Collections.Generic;

namespace Core.DomainModel
{
    public class ItProjectModuleName : IEntity<int>
    {
        public ItProjectModuleName()
        {
            this.Configs = new List<Config>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Config> Configs { get; set; }
    }
}