﻿using System;
using System.Collections.Generic;
using Core.DomainModel;

namespace UI.MVC4.Models
{
    public class ItInterfaceDTO
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string ItInterfaceId { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string ObjectOwnerName { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public int? ExhibitedById { get; set; }
        public int? ExhibitedByItSystemId { get; set; }
        public string ExhibitedByItSystemName { get; set; }
        public int? TsaId { get; set; }
        public string TsaName { get; set; }
        public int? InterfaceId { get; set; }
        public string InterfaceName { get; set; }
        public int? InterfaceTypeId { get; set; }
        public string InterfaceTypeName { get; set; }
        public int? MethodId { get; set; }
        public string MethodName { get; set; }
        public IEnumerable<DataRowDTO> DataRows { get; set; }
        public int? BelongsToId { get; set; }
        public string BelongsToName { get; set; }
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets whether this instance has a usage in any organization.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a usage; otherwise, <c>false</c>.
        /// </value>
        public bool IsUsed { get; set; }
    }
}
