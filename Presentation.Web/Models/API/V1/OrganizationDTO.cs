﻿using System;
using Core.DomainModel;

namespace Presentation.Web.Models.API.V1
{
    public class OrganizationDTO : NamedEntityDTO
    {
        public string Phone { get; set; }
        public string Adress { get; set; }
        public string Email { get; set; }
        public string Cvr { get; set; }
        public string ForeignCvr { get; set; }

        public int TypeId { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public ConfigDTO Config { get; set; }

        public OrgUnitSimpleDTO Root { get; set; }
        public DateTime LastChanged { get; set; }
        public int LastChangedByUserId { get; set; }
        public Guid Uuid { get; set; }

        public virtual int? ContactPersonId { get; set; }
        public virtual UserDTO ContactPerson { get; set; }
    }
}
