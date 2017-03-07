using Core.DomainModel;
using Core.DomainModel.Advice;
using System.Collections.Generic;

namespace Core.ApplicationServices
{
    public interface IAdviceService
    {
        bool sendAdvice(int id);
        IEnumerable<Advice> GetAdvicesForOrg(int orgKey);
    }
}
