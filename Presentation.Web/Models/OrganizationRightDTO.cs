﻿using Core.DomainModel;
using System;

namespace Presentation.Web.Models
{
    public class OrganizationRightDTO
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public OrganizationRole Role { get; set; }

        public int UserId { get; set; }
        public UserDTO User { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        public DateTime LastChanged { get; set; }
        public int LastChangedByUserId { get; set; }

        public string ObjectOwnerName { get; set; }
        public string ObjectOwnerLastName { get; set; }
        public string ObjectOwnerFullName
        {
            get { return ObjectOwnerName + " " + ObjectOwnerLastName; }
        }
        public string DefaultOrgUnitName { get; set; }
    }
}
