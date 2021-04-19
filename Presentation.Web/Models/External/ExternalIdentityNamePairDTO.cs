using System;

namespace Presentation.Web.Models.External
{
    public class ExternalIdentityNamePairDTO
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }

        public ExternalIdentityNamePairDTO(Guid uuid, string name)
        {
            Uuid = uuid;
            Name = name;
        }
    }
}