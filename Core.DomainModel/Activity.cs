using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public class Activity : IEntity<int>
    {
        public int Id { get; set; }

        /// <summary>
        /// Human readable ID ("brugervendt noegle" in OIO)
        /// </summary>
        public string AktivityId { get; set; }
        public string Name { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Associated activities
        /// </summary>
        public virtual ICollection<Activity> AssociatedActivities { get; set; }

        public int AssociatedUserId { get; set; }
        public virtual User AssociatedUser { get; set; }
    }
}
