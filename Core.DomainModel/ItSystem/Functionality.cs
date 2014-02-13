using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class Functionality
    {
        public Functionality()
        {
            this.Wishes = new List<Wish>();
        }

        public int Id { get; set; }

        public virtual ICollection<Wish> Wishes { get; set; }
        public virtual ItSystem ItSystem { get; set; }
    }
}
