using System.Collections.Generic;
using Core.DomainModel;

namespace Core.DomainServices.Repositories.GDPR
{
    public interface IItSystemUsageAttachedOptionRepository
    {
        IEnumerable<AttachedOption> GetBySystemUsageId(int systemUsageId);
        IEnumerable<AttachedOption> GetBySystemUsageIdAndOptionType(int systemUsageId, OptionType type);
        void DeleteAllBySystemUsageId(int systemUsageId);
        void DeleteAttachedOption(int systemUsageId, int optionId, OptionType optionType);
        void AddAttachedOption(int systemUsageId, int optionId, OptionType optionType);
    }
}
