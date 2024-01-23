using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;

namespace Presentation.Web.Models.API.V2.Request.System.RightsHolder
{
    /// <summary>
    /// Defines a full it-system definition used for updates and replacement operations
    /// </summary>
    public class RightsHolderFullItSystemRequestDTO : IRightsHolderWritableSystemPropertiesRequestDTO, IHasExternalReferencesCreation
    {
        /// <summary>
        /// UUID for owning organization
        /// </summary>
        /// <remarks>Use api/v2/rightsholder/organizations API for getting a list of possible organizations related to the logged in user</remarks>
        [Required]
        [NonEmptyGuid]
        public Guid RightsHolderUuid { get; set; }
        
        /// <summary>
        /// External Uuid for IT-System
        /// Note: When setting ExternalUuid to NULL there's no way to tell if the value was removed or if it was never set.
        /// </summary>
        [NonEmptyGuid]
        public Guid? ExternalUuid{ get; set; }

        /// <summary>
        /// UUID for possible IT-System parent (if any)
        /// </summary>
        [NonEmptyGuid]
        public Guid? ParentUuid { get; set; }

        /// <summary>
        /// Name of IT-System
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [MaxLength(Core.DomainModel.ItSystem.ItSystem.MaxNameLength)]
        public string Name { get; set; }

        /// <summary>
        /// Former name of IT-System (if any)
        /// </summary>
        public string PreviousName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Description { get; set; }
        
        /// <summary>
        /// User defined external references.
        /// The external reference marked as "master reference" will be shown in overviews
        /// Constraints:
        ///     - If the list is not empty one (and only one) must be marked as the master reference.
        ///     - If the reference has a uuid it will update an existing reference (with the same uuid), uuid must exist
        ///     - If the reference has no uuid, a new External Reference will be created
        ///     - Existing references will be replaced by the input data, so unless identified using uuid in the updates, the existing references will be removed.
        /// </summary>
        public IEnumerable<ExternalReferenceDataWriteRequestDTO> ExternalReferences { get; set; }

        /// <summary>
        /// UUID for IT-System business type
        /// </summary>
        [NonEmptyGuid]
        public Guid? BusinessTypeUuid { get; set; }

        /// <summary>
        /// UUID's for KLE numbers categorizing this IT-System
        /// </summary>
        public IEnumerable<Guid> KLEUuids { get; set; }
    }
}