using Presentation.Web.Infrastructure.Attributes;
using System;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Request.System.Regular
{
    public interface IItSystemWriteRequestPropertiesDTO
    {
        [NonEmptyGuid]
        public Guid? RightsHolderUuid { get; set; }

        /// <summary>
        /// The scope of the IT-System masterdata
        /// </summary>
        public RegistrationScopeChoice? Scope { get; set; }

        /// <summary>
        /// Archive duty recommendation from "Rigsarkivet"
        /// </summary>
        public RecommendedArchiveDutyRequestDTO RecommendedArchiveDuty { get; set; }
        /// <summary>
        /// Determines if the system has been deactivated from being taken into use
        /// </summary>
        public bool Deactivated { get; set; }
    }
}
