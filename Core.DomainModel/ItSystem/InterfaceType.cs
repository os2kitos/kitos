using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core.DomainModel.ItSystem
{
    public class InterfaceType : IEntity<int>
    {
        public InterfaceType()
        {
            this.ItSystems = new Collection<ItSystem>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }

        public virtual ICollection<ItSystem> ItSystems { get; set; }
    }
}