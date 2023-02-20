using System;

namespace Core.ApplicationServices.Model.System
{
    public class SystemCreationParameters : SystemUpdateParameters
    {
        public Guid? RightsHolderProvidedUuid { get; set; }
    }
}
