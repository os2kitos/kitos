using System;

namespace Presentation.Web.Models.References
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

        public NamedEntityDTO CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}