using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.External.V2.Request
{
    public class RightsHolderCreateItInterfaceRequestDTO : RightsHolderWritableItInterfacePropertiesDTO
    {
        /// <summary>
        /// UUID for owning organization
        /// </summary>
        /// <remarks>Use api/v2/rightsholder/organizations API for getting a list of possible organizations related to the logged in user</remarks>
        [Required]
        public Guid RightsHolderUuid { get; set; }

        /// <summary>
        /// UUID for IT-Interface
        /// If no UUID is provided, KITOS will assign one.
        /// </summary>
        public Guid? Uuid { get; set; }
    }
}