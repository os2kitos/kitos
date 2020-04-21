using Core.DomainModel;

namespace Core.DomainServices.Context
{
    /// <summary>
    /// Defines how to resolve a user which can be used in data modification operations when no active user is present
    /// </summary>
    public interface IFallbackUserResolver
    {
        User Resolve();
    }
}
