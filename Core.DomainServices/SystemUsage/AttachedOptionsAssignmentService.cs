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
        private readonly IAttachedOptionRepository _attachedOptionRepository;
        private readonly IOptionsService<TTarget, TOption> _optionsService;

        public AttachedOptionsAssignmentService(OptionType optionType, IAttachedOptionRepository attachedOptionRepository, IOptionsService<TTarget, TOption> optionsService)
        {
            _optionType = optionType;
            _attachedOptionRepository = attachedOptionRepository;
            _optionsService = optionsService;
        }

        public Result<IEnumerable<TOption>, OperationError> UpdateAssignedOptions(ItSystemUsage systemUsage, IEnumerable<Guid> optionUuids)
        {
            if (systemUsage == null)
                throw new ArgumentNullException(nameof(systemUsage));

            var ids = optionUuids?.ToList() ?? throw new ArgumentNullException(nameof(optionUuids));

            if (ids.Count != ids.Distinct().Count())
                return new OperationError($"Duplicates {_optionType:G} are not allowed", OperationFailure.BadInput);

            var existingIds = _attachedOptionRepository
                .GetBySystemUsageIdAndOptionType(systemUsage.Id, _optionType)
                .Select(x => x.OptionId)
                .ToHashSet();

            var personalDataTypes = new List<TOption>();
            foreach (var uuid in ids)
            {
                var optionResult = _optionsService.GetOptionByUuid(systemUsage.OrganizationId, uuid);
                if (optionResult.IsNone)
                    return new OperationError($"{_optionType:G} with id:{uuid} does not exist", OperationFailure.BadInput);

                //Only apply org availability constraint if the type was added (compared to current state)
                var type = optionResult.Value.option;

                if (!existingIds.Contains(type.Id) && !optionResult.Value.available)
                    return new OperationError($"{_optionType:G} with id:{uuid} is not available in the organization", OperationFailure.BadInput);

                personalDataTypes.Add(type);
            }

            //Compute deltas and apply changes
            var typesToRemove = existingIds.Except(personalDataTypes.Select(x => x.Id)).ToList();
            var typesToAdd = personalDataTypes.Select(x => x.Id).Except(existingIds).ToList();

            foreach (var id in typesToRemove)
                _attachedOptionRepository.DeleteAttachedOption(systemUsage.Id, id, _optionType);

            foreach (var id in typesToAdd)
                _attachedOptionRepository.AddAttachedOption(systemUsage.Id, id, _optionType);

            return personalDataTypes;
        }
    }
}
