using System.Collections.Generic;

namespace Core.DomainModel
{
    public interface IHasReferences
    {
       ICollection<ExternalReference>  ExternalReferences { get; set; }
    }
}
