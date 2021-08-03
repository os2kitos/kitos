﻿using System;
using Presentation.Web.Models.External.V2.Types;

namespace Presentation.Web.Models.External.V2.Response.Organization
{
    public class OrganizationResponseDTO: ShallowOrganizationResponseDTO
    {
        public OrganizationType OrganizationType { get; }
        public OrganizationResponseDTO(Guid uuid, string name, string cvr, OrganizationType organizationType) : base(uuid, name, cvr)
        {
            OrganizationType = organizationType;
        }
    }
}