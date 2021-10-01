using System;

namespace Presentation.Web.Models.API.V2.Request.Interface
{
    public interface IRightsHolderWritableItInterfacePropertiesDTO
    {
        public Guid ExposedBySystemUuid { get; set; }
        public string Name { get; set; }
        public string InterfaceId { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string UrlReference { get; set; }
    }
}
