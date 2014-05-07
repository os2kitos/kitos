using System;

namespace UI.MVC4.Models
{
    public class ActivityDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// Human readable ID ("brugervendt noegle" in OIO)
        /// </summary>
        public string ActivityId { get; set; }
        public string Name { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int? AssociatedUserId { get; set; }
        public UserDTO AssociatedUser { get; set; }

        public int ObjectOwnerId { get; set; }
        public UserDTO ObjectOwner { get; set; }
    }
}