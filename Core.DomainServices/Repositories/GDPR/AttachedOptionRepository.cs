using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices.Context;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.GDPR
{
    public class AttachedOptionRepository : IAttachedOptionRepository
    {
        private readonly IGenericRepository<AttachedOption> _attachedOptionRepository;
        private readonly Maybe<ActiveUserIdContext> _userContext;

        public AttachedOptionRepository(IGenericRepository<AttachedOption> attachedOptionRepository, Maybe<ActiveUserIdContext> userContext)
        {
            _attachedOptionRepository = attachedOptionRepository;
            _userContext = userContext;
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

        public void DeleteAttachedOption(int systemUsageId, int optionId, OptionType optionType)
        {
            var option = GetOption(systemUsageId, optionId, optionType);

            if (option.HasValue)
                _attachedOptionRepository.Delete(option.Value);

            _attachedOptionRepository.Save();
        }

        private Maybe<AttachedOption> GetOption(int systemUsageId, int optionId, OptionType optionType)
        {
            return _attachedOptionRepository.AsQueryable().FirstOrDefault(x =>
                x.OptionType == optionType &&
                x.ObjectType == EntityType.ITSYSTEMUSAGE &&
                x.OptionId == optionId &&
                x.ObjectId == systemUsageId).FromNullable();
        }

        public void AddAttachedOption(int systemUsageId, int optionId, OptionType optionType)
        {
            var option = GetOption(systemUsageId, optionId, optionType);
            if (option.IsNone)
            {
                _attachedOptionRepository.Insert(new AttachedOption
                {
                    OptionType = optionType,
                    OptionId = optionId,
                    ObjectType = EntityType.ITSYSTEMUSAGE,
                    ObjectId = systemUsageId,
                    ObjectOwnerId = _userContext.Select(x => x.ActiveUserId).GetValueOrDefault()
                });
            }
        }
    }
}
