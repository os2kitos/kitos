using System.Collections.Generic;

namespace Core.DomainModel
{
    public interface IHasRights<TRight>
    {
        ICollection<TRight> Rights { get; set; }
    }
}