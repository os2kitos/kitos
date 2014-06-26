using System.Collections.Generic;

namespace Core.DomainModel
{
    /// <summary>
    /// Implemented by types whose instances are editable by the global administrator,
    /// and for which new instances can be suggested by local admin. 
    /// </summary>
    /// <typeparam name="TReference">
    /// Type of the other end of the relationsship, 
    /// which holds a collection of this type.
    /// </typeparam>
    public interface IOptionEntity<TReference>
    {
        string Name { get; set; }
        bool IsActive { get; set; }
        bool IsSuggestion { get; set; }
        string Note { get; set; }

        ICollection<TReference> References { get; set; }
    }
}