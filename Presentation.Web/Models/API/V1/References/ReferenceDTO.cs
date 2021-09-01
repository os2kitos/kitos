using System;

namespace Presentation.Web.Models.API.V1.References
{
    public class ReferenceDTO : NamedEntityDTO
    {
        public ReferenceDTO(int id, string name)
            : base(id, name)
        {

        }

        public string ReferenceId { get; set; }

        public string Url { get; set; }

        public bool MasterReference { get; set; }

        public NamedEntityDTO LastChangedByUser { get; set; }

        public DateTime LastChanged { get; set; }
    }
}