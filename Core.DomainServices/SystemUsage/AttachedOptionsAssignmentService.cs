using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.GDPR;

namespace Core.DomainServices.SystemUsage
{
    public class AttachedOptionsAssignmentService<TOption, TTarget> : IAttachedOptionsAssignmentService<TOption, TTarget> where TOption : OptionEntity<TTarget>
    {
        private readonly OptionType _optionType;
        private readonly IItSystemUsageAttachedOptionRepository _itSystemUsageAttachedOptionRepository;
        private readonly IOptionsService<TTarget, TOption> _optionsService;

        public AttachedOptionsAssignmentService(OptionType optionType, IItSystemUsageAttachedOptionRepository itSystemUsageAttachedOptionRepository, IOptionsService<TTarget, TOption> optionsService)
        {
            _optionType = optionType;
            _itSystemUsageAttachedOptionRepository = itSystemUsageAttachedOptionRepository;
            _optionsService = optionsService;
        }

        public Result<IEnumerable<TOption>, OperationError> UpdateAssignedOptions(ItSystemUsage systemUsage, IEnumerable<Guid> optionUuids)
        {
            if (systemUsage == null)
                throw new ArgumentNullException(nameof(systemUsage));

            var ids = optionUuids?.ToList() ?? throw new ArgumentNullException(nameof(optionUuids));

            if (ids.Count != ids.Distinct().Count())
                return new OperationError($"Duplicates {_optionType:G} are not allowed", OperationFailure.BadInput);

            var existingIds = _itSystemUsageAttachedOptionRepository
                .GetBySystemUsageIdAndOptionType(systemUsage.Id, _optionType)
                .Select(x => x.OptionId)
                .ToHashSet();

            var types = new List<TOption>();
            foreach (var uuid in ids)
            {
                var optionResult = _optionsService.GetOptionByUuid(systemUsage.OrganizationId, uuid);
                if (optionResult.IsNone)
                    return new OperationError($"{_optionType:G} with id:{uuid} does not exist", OperationFailure.BadInput);

                //Only apply org availability constraint if the type was added (compared to current state)
                var type = optionResult.Value.option;

                if (!existingIds.Contains(type.Id) && !optionResult.Value.available)
                    return new OperationError($"{_optionType:G} with id:{uuid} is not available in the organization", OperationFailure.BadInput);

                types.Add(type);
            }

            //Compute deltas and apply changes
            var typesToRemove = existingIds.Except(types.Select(x => x.Id)).ToList();
            var typesToAdd = types.Select(x => x.Id).Except(existingIds).ToList();

            foreach (var id in typesToRemove)
                _itSystemUsageAttachedOptionRepository.DeleteAttachedOption(systemUsage.Id, id, _optionType);

            foreach (var id in typesToAdd)
                _itSystemUsageAttachedOptionRepository.AddAttachedOption(systemUsage.Id, id, _optionType);

            return types;
        }
    }
}
