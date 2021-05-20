using System.Collections.Generic;
using Core.DomainModel.Advice;

namespace Core.DomainServices.Repositories.Advice
{
    public interface IAdviceRepository
    {
        IEnumerable<DomainModel.Advice.Advice> GetByRelationIdAndType(int relationId, ObjectType objectType);
    }
}
