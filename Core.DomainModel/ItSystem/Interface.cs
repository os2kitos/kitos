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
        public int ItSystem_Id { get; set; }
        public virtual ICollection<Wish> Wishes { get; set; }
        public virtual ItSystem ItSystem { get; set; }
    }
}
