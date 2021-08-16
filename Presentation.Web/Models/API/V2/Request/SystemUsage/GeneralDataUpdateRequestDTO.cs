using System;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class GeneralDataUpdateRequestDTO : GeneralDataWriteRequestDTO
    {
        /// <summary>
        /// Defines the master contract for this system (many contracts can point to a system usage but only one can be the master contract)
        /// Constraint: The contract provided MUST point to this system usage for it to be selected as "main contract".
        /// </summary>
        [NonEmptyGuid]
        public Guid? MainContractUuid { get; set; }
    }
}