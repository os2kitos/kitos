using System;
using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class Handover : IEntity<int>
    {
        public Handover()
        {
            this.Participants = new List<User>();
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string Summary { get; set; }

        public virtual ItProject ItProject { get; set; }
        public virtual ICollection<User> Participants { get; set; }
    }
}
