using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.System
{
    public interface IRightsHolderWritableSystemPropertiesRequestDTO
    {
        public Guid? ParentUuid { get; set; }
        public string Name { get; set; }
        public string FormerName { get; set; }
        public string Description { get; set; }
        public string UrlReference { get; set; }
        public Guid? BusinessTypeUuid { get; set; }
        public IEnumerable<string> KLENumbers { get; set; }
        public IEnumerable<Guid> KLEUuids { get; set; }
    }
}
