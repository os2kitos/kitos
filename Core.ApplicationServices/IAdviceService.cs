using Core.DomainModel.Advice;
using System.Collections.Generic;

namespace Core.ApplicationServices
{
    public interface IAdviceService
    {
        bool SendAdvice(int id);
        IEnumerable<Advice> GetAdvicesForOrg(int orgKey);
    }
}
