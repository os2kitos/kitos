using System.Collections.Generic;

namespace Core.DomainModel
{
    public interface IHasUsages
    {
        ICollection<ItSystemUsage.ItSystemUsage> Usages { get; set; }
    }
}
