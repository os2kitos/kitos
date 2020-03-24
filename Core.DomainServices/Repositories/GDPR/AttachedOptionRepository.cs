using System.Collections.Generic;
using Core.DomainModel;

namespace Core.DomainServices.Repositories.GDPR
{
    public class AttachedOptionRepository : IAttachedOptionRepository
    {
        private readonly IGenericRepository<AttachedOption> _attachedOptionRepository;

        public AttachedOptionRepository(IGenericRepository<AttachedOption> attachedOptionRepository)
        {
            _attachedOptionRepository = attachedOptionRepository;
        }

        public IEnumerable<AttachedOption> GetAttachedOptions()
        {
            return _attachedOptionRepository.Get();
        }
    }
    
}
