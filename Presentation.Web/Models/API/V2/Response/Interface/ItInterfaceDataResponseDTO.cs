using System;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Response.Interface
{
    public class ItInterfaceDataResponseDTO : IHasUuidExternal
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
        public IdentityNamePairResponseDTO DataType { get; set; }
        /// <summary>
        /// UUID of the data description
        /// </summary>
        public Guid Uuid { get; set; }
    }
}