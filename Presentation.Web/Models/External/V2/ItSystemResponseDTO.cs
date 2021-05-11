﻿using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.External.V2
{
    public class ItSystemResponseDTO
    {
        /// <summary>
        /// UUID for IT-System
        /// </summary>
        public Guid Uuid { get; set; }

        /// <summary>
        /// UUID for possible IT-System parent (if any)
        /// </summary>
        public Guid? ParentSystemUuid { get; set; }
        
        /// <summary>
        /// Name of IT-System
        /// </summary>
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
        /// Url reference for further information
        /// </summary>
        public string UrlReference { get; set; }

        /// <summary>
        /// List of KLE number representations as name and UUID pairs
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> KLE { get; set; }

        /// <summary>
        /// Active status
        /// </summary>
        public bool Deactivated { get; set; }

        /// <summary>
        /// Name and UUID pair for IT-System business type
        /// </summary>
        public IdentityNamePairResponseDTO BusinessType { get; set; }

        /// <summary>
        /// Organizational information for IT-System rightsholder
        /// </summary>
        public OrganizationResponseDTO RightsHolder { get; set; }

        /// <summary>
        /// List of IT-Interfaces exposed by this IT-System
        /// </summary>
        public IEnumerable<Guid> ExposedInterfaces { get; set; }

        /// <summary>
        /// Date of creation
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Responsible for creation
        /// </summary>
        public IdentityNamePairResponseDTO CreatedBy { get; set; }

        /// <summary>
        /// Time of last modification
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Responsible for last modification
        /// </summary>
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }

        /// <summary>
        /// Comment for recommended archive duty
        /// </summary>
        public string RecommendedArchiveDutyComment { get; set; }

        /// <summary>
        /// Name and id pair detailing the archive duty
        /// </summary>
        public RecommendedArchiveDutyResponseDTO RecommendedArchiveDutyResponse { get; set; }
    }
}