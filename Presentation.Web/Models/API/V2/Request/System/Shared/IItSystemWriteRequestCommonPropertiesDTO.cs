using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.System.Shared
{
    public interface IItSystemWriteRequestCommonPropertiesDTO
    {
        public Guid? ParentUuid { get; set; }
        public string Name { get; set; }
        public string PreviousName { get; set; }
        public string Description { get; set; }
        public Guid? BusinessTypeUuid { get; set; }
        public IEnumerable<Guid> KLEUuids { get; set; }
    }
}
