using System;
using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class Handover : Entity
    {
        public Handover()
        {
            this.Participants = new List<User>();
        }
        
        public string Description { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string Summary { get; set; }

        public virtual ItProject ItProject { get; set; }
        public virtual ICollection<User> Participants { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (ItProject != null && ItProject.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
