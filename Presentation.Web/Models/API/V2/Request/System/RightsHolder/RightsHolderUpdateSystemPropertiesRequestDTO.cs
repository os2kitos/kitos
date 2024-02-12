using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;

namespace Presentation.Web.Models.API.V2.Request.System.RightsHolder
{
    public class RightsHolderUpdateSystemPropertiesRequestDTO : IRightsHolderWritableSystemPropertiesRequestDTO, IHasExternalReferencesUpdate
    {
        /// <summary>
        /// UUID for possible IT-System parent (if any)
        /// </summary>
        [NonEmptyGuid]
        public Guid? ParentUuid { get; set; }

        /// <summary>
        /// External Uuid for IT-System
        /// </summary>
        [NonEmptyGuid]
        public Guid? ExternalUuid { get; set; }

        /// <summary>
        /// Name of IT-System
        /// </summary>
        [MaxLength(Core.DomainModel.ItSystem.ItSystem.MaxNameLength)]
        public string Name { get; set; }

        /// <summary>
        /// Former name of IT-System (if any)
        /// </summary>
        public string PreviousName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
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
        public IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> ExternalReferences { get; set; }

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