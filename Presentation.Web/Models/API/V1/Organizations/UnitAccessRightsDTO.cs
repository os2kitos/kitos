﻿namespace Presentation.Web.Models.API.V1.Organizations
{
    public class UnitAccessRightsDTO
    {
        public UnitAccessRightsDTO()
        {
        }

        public UnitAccessRightsDTO(bool canBeRead, bool canBeModified, bool canNameBeModified, bool canEanBeModified, bool canDeviceIdBeModified, bool canBeRearranged, bool canBeDeleted)
        {
            CanBeRead = canBeRead;
            CanBeModified = canBeModified;
            CanNameBeModified = canNameBeModified;
            CanEanBeModified = canEanBeModified;
            CanDeviceIdBeModified = canDeviceIdBeModified;
            CanBeRearranged = canBeRearranged;
            CanBeDeleted = canBeDeleted;
        }

        protected UnitAccessRightsDTO(UnitAccessRightsDTO other)
        : this(other.CanBeRead, other.CanBeModified, other.CanNameBeModified, other.CanEanBeModified, other.CanDeviceIdBeModified, other.CanBeRearranged, other.CanBeDeleted)
        {

        }

        public bool CanBeRead { get; set; }
        public bool CanBeModified { get; set; }
        public bool CanNameBeModified { get; set; }
        public bool CanEanBeModified { get; set; }
        public bool CanDeviceIdBeModified { get; set; }
        public bool CanBeRearranged { get; set; }
        public bool CanBeDeleted { get; set; }
    }
}