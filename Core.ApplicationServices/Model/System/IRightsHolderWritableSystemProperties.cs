using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.System
{
    public interface IRightsHolderWritableSystemProperties
    {
        public string Name { get; }
        public Guid? ParentSystemUuid { get; }
        public string FormerName { get; }
        public string Description { get; }
        public string UrlReference { get; }
        public Guid? BusinessTypeUuid { get; }
        public IEnumerable<string> TaskRefKeys { get; }
        public IEnumerable<Guid> TaskRefUuids { get; }
    }
}
