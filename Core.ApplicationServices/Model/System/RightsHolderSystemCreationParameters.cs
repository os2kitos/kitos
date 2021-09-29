using System;

namespace Core.ApplicationServices.Model.System
{
    public class RightsHolderSystemCreationParameters : RightsHolderSystemUpdateParameters
    {
        public Guid? RightsHolderProvidedUuid { get; set; }
    }
}
