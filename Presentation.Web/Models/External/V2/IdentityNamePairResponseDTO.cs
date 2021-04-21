using System;

namespace Presentation.Web.Models.External.V2
{
    public class IdentityNamePairResponseDTO
    {
        /// <summary>
        /// UUID
        /// </summary>
        public Guid Uuid { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        public IdentityNamePairResponseDTO(Guid uuid, string name)
        {
            Uuid = uuid;
            Name = name;
        }
    }
}