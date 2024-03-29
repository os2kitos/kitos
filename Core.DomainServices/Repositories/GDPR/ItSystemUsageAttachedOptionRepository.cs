﻿using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices.Context;


namespace Core.DomainServices.Repositories.GDPR
{
    public class ItSystemUsageAttachedOptionRepository : IItSystemUsageAttachedOptionRepository
    {
        private readonly IGenericRepository<AttachedOption> _attachedOptionRepository;
        private readonly Maybe<ActiveUserIdContext> _userContext;

        public ItSystemUsageAttachedOptionRepository(IGenericRepository<AttachedOption> attachedOptionRepository, Maybe<ActiveUserIdContext> userContext)
        {
            _attachedOptionRepository = attachedOptionRepository;
            _userContext = userContext;
        }

        public IEnumerable<AttachedOption> GetBySystemUsageId(int systemUsageId)
        {
            return GetAttachedOptionsBySystemUsageId(systemUsageId).ToList();
        }

        private IQueryable<AttachedOption> GetAttachedOptionsBySystemUsageId(int systemUsageId)
        {
            return _attachedOptionRepository
                .AsQueryable()
                .Where(x => x.ObjectType == EntityType.ITSYSTEMUSAGE && x.ObjectId == systemUsageId);
        }

        public IEnumerable<AttachedOption> GetBySystemUsageIdAndOptionType(int systemUsageId, OptionType type)
        {
            return GetAttachedOptionsBySystemUsageId(systemUsageId).Where(x => x.OptionType == type).ToList();
        }

        public void DeleteAllBySystemUsageId(int systemUsageId)
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
