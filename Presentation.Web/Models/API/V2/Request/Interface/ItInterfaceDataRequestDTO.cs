using System;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.Interface
{
    public class ItInterfaceDataRequestDTO
    {
        /// <summary>
        /// Optional description of the data
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Optional reference to the it-interface-data-type option type
        /// Constraint:
        /// - If changing from previous state, the newly selected option must be available in the organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? DataTypeUuid { get; set; }
    }
}