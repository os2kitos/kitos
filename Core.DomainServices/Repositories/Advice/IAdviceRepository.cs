using System.Collections.Generic;
using Core.DomainModel.Shared;

namespace Core.DomainServices.Repositories.Advice
{
    public interface IAdviceRepository
    {
        IEnumerable<DomainModel.Advice.Advice> GetByRelationIdAndType(int relationId, RelatedEntityType objectType);
    }
}
