﻿using Core.DomainModel.Shared;
using Presentation.Web.Models.References;

namespace Presentation.Web.Models.GDPR
{
    public class DataProcessingRegistrationDTO : NamedEntityDTO
    {
        public DataProcessingRegistrationDTO(int id, string name)
            : base(id, name)
        {
        }

        public AssignedRoleDTO[] AssignedRoles { get; set; }

        public ReferenceDTO[] References { get; set; }

        public NamedEntityWithEnabledStatusDTO[] ItSystems { get; set; }

        public YearMonthIntervalOption? OversightIntervalOption { get; set; }

        public string OversightIntervalOptionNote { get; set; }
        
        public ShallowOrganizationDTO[] DataProcessors { get; set; }
    }
}