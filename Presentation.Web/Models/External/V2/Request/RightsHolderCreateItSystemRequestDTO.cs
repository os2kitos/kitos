using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.External.V2.Request
{
    public class RightsHolderCreateItSystemRequestDTO : RightsHolderWritableITSystemPropertiesDTO
    {
        /// <summary>
        /// UUID for owning organization
        /// </summary>
        /// <remarks>Use api/v2/rightsholder/organizations API for getting a list of possible organizations related to the logged in user</remarks>
        [Required]
        [NonEmptyGuid]
        public Guid RightsHolderUuid { get; set; }

        /// <summary>
        /// UUID for IT-System
        /// If no uuid is provided, KITOS will assign one automatically
        /// </summary>
        [NonEmptyGuid]
        public Guid? Uuid { get; set; }
    }
}