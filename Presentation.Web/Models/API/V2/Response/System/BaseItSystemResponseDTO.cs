using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Response.System
{
    public abstract class BaseItSystemResponseDTO : IHasNameExternal, IHasUuidExternal, IHasEntityCreator, IHasDeactivatedExternal
    {
        /// <summary>
        /// UUID for IT-System
        /// </summary>
        [Required]
        public Guid Uuid { get; set; }


        /// <summary>
        /// External Uuid for IT-System
        /// </summary>
        public Guid? ExternalUuid { get; set; }

        /// <summary>
        /// UUID for possible IT-System parent (if any)
        /// </summary>
        public IdentityNamePairResponseDTO ParentSystem { get; set; }

        /// <summary>
        /// Name of IT-System
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Former name of IT-System (if any)
        /// </summary>
        public string FormerName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// User defined external references
        /// </summary>
        [Required]
        public IEnumerable<ExternalReferenceDataResponseDTO> ExternalReferences { get; set; }

        /// <summary>
        /// List of KLE number representations as name and UUID pairs
        /// </summary>
        [Required]
        public IEnumerable<IdentityNamePairResponseDTO> KLE { get; set; }

        /// <summary>
        /// Active status
        /// </summary>
        [Required]
        public bool Deactivated { get; set; }

        /// <summary>
        /// Name and UUID pair for IT-System business type
        /// </summary>
        public IdentityNamePairResponseDTO BusinessType { get; set; }

        /// <summary>
        /// Organizational information for IT-System rightsholder
        /// </summary>
        public ShallowOrganizationResponseDTO RightsHolder { get; set; }

        /// <summary>
        /// Date of creation (on some legacy systems , this information is not available. If so, it will be null)
        /// </summary>
        [Required]
        public DateTime? Created { get; set; }

        /// <summary>
        /// Responsible for creation.
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO CreatedBy { get; set; }

        /// <summary>
        /// Archive duty recommendation from "Rigsarkivet"
        /// </summary>
        [Required]
        public RecommendedArchiveDutyResponseDTO RecommendedArchiveDuty { get; set; }
        /// <summary>
        ///A list of unique suppliers associated with each usage’s main contract.
        /// </summary>
        [Required]
        public IEnumerable<IdentityNamePairResponseDTO> MainContractSuppliers { get; set; }
    }
}