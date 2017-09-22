using System.Collections.Generic;

namespace Core.DomainModel
{
    public interface IOptionReference<TReference>
    {
        /// <summary>
        /// Gets or sets the references to an object which uses this option.
        /// </summary>
        /// <value>
        /// The references.
        /// </value>
        ICollection<TReference> References { get; set; }
    }
}
