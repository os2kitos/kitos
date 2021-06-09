using System;

namespace Presentation.Web.Models.External.V2
{
    public class AvailableNamePairResponseDTO
    {
        /// <summary>
        /// UUID
        /// </summary>
        public Guid Uuid { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// IsAvailable
        /// </summary>
        public bool IsAvailable { get; set; }

        public AvailableNamePairResponseDTO(Guid uuid, string name, bool isAvailable)
        {
            Uuid = uuid;
            Name = name;
            IsAvailable = isAvailable;
        }
    }
}