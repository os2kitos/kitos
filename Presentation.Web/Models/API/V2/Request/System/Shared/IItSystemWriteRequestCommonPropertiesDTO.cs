using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.System.Shared
{
    public interface IItSystemWriteRequestCommonPropertiesDTO
    {
        public Guid? ParentUuid { get; set; }
        public string Name { get; set; }
        public string FormerName { get; set; }
        public string Description { get; set; }
        public string UrlReference { get; set; } //TODO: expose a proper external references interface
        public Guid? BusinessTypeUuid { get; set; }
        public IEnumerable<Guid> KLEUuids { get; set; }
    }
}
