using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Context
{
    /// <summary>
    /// Defines how to resolve a user which can be used in data modification operations when no active user is present
    /// </summary>
    public class FallbackUserResolver : IFallbackUserResolver
    {
        private readonly IGenericRepository<User> _userRepository;

        public FallbackUserResolver(IGenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public User Resolve()
        {
            //Get the first available global admin since we don't have the concept of a system user
            return _userRepository
                .AsQueryable()
                .Where(x => x.IsGlobalAdmin)
                .OrderBy(x => x.Id)
                .First();
        }
    }
}
