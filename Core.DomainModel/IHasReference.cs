using System.Collections.Generic;

namespace Core.DomainModel
{
    public interface IHasReferences
    {
       ExternalReference Reference { get; set; }
       ICollection<ExternalReference>  ExternalReferences { get; set; }
    }
}
