﻿using System;

namespace Presentation.Web.Models
{
    public class ExternalReferenceDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ExternalReferenceId { get; set; }
        public string URL { get; set; }
        public Core.DomainModel.Display Display { get; set; }
        public int ObjectOwnerId { get; set; }
        public virtual UserDTO ObjectOwner { get; set; }
        public int? ItProject_Id { get; set; }
        public int? ItContract_Id { get; set; }
        public int? ItSystemUsage_Id { get; set; }
        public int? ItSystem_Id { get; set; }
        public DateTime Created { get; set; }
    }
}