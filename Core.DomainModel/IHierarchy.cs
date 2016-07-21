using System.Collections.Generic;

namespace Core.DomainModel
{
    /// <summary>
    /// Marker interface for entities with hierarchical data
    /// </summary>
    /// <typeparam name="TSelf">The class of the implementing class</typeparam>
    /// <remarks>
    /// C# isn't sophisticated enough to use self.
    /// So a workaround using TSelf is used.
    /// http://stackoverflow.com/questions/8202384/how-to-reference-the-implementing-class-from-an-interface
    /// </remarks>
    public interface IHierarchy<TSelf>
        where TSelf : class
    {
        int? ParentId { get; set; }
        TSelf Parent { get; set; }
        ICollection<TSelf> Children { get; set; }
    }
}