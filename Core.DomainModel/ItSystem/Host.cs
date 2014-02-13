using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class Host
    {
        public Host()
        {
            this.Infrastructures = new List<Infrastructure>();
        }

        public int Id { get; set; }
        public virtual ICollection<Infrastructure> Infrastructures { get; set; }
    }
}
