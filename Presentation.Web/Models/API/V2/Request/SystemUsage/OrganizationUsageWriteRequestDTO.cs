using System;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class OrganizationUsageWriteRequestDTO
    {
        /// <summary>
        /// UUIds of Organization units using this system
        /// </summary>
        public IEnumerable<Guid> UsingOrganizationUnitUuids { get; set; }
        /// <summary>
        /// Out of all of the using organization units, this one is responsible for the system within the organization.
        /// Constraint: The uuid provided must also be present in UsingOrganizationUnitUuids
        /// </summary>
        [NonEmptyGuid]
        public Guid? ResponsibleOrganizationUnitUuid { get; set; }
    }
}