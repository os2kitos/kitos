using System.Collections.Generic;

namespace Core.DomainModel
{
    public class ItSystemModuleName : IEntity<int>
    {
        public ItSystemModuleName()
        {
            this.Configs = new List<Config>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Config> Configs { get; set; }
    }
}