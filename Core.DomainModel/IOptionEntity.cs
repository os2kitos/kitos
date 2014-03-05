using System.Collections.Generic;

namespace Core.DomainModel
{
    public interface IOptionEntity<T> : IEntity<int>
    {
        string Name { get; set; }
        bool IsActive { get; set; }
        bool IsSuggestion { get; set; }
        string Note { get; set; }

        ICollection<T> References { get; set; }
    }
}