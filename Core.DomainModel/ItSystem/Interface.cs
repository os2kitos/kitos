using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class Interface
    {
        public Interface()
        {
            this.Wishes = new List<Wish>();
        }

        public int Id { get; set; }
        public int ItSystemId { get; set; }
        public int MethodId { get; set; }
        
        public virtual ICollection<Wish> Wishes { get; set; }
        public virtual ItSystem ItSystem { get; set; }
        public virtual Method Method { get; set; }
    }
}
