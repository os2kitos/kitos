using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<AttachedOption> GetBySystemUsageId(int systemUsageId)
        {
            return _attachedOptionRepository
                .AsQueryable()
                .Where(x => x.ObjectType == EntityType.ITSYSTEMUSAGE && x.ObjectId == systemUsageId)
                .ToList();
        }

        public void DeleteBySystemUsageId(int systemUsageId)
        {
            var attachedOptions = GetBySystemUsageId(systemUsageId);

            foreach (var attachedOption in attachedOptions)
                _attachedOptionRepository.Delete(attachedOption);

            _attachedOptionRepository.Save();
        }
    }
}
