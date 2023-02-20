using System;

namespace Core.ApplicationServices.Model.System
{
    public class RightsHolderSystemCreationParameters : SharedSystemUpdateParameters
    {
        public Guid? RightsHolderProvidedUuid { get; set; }
    }
}
