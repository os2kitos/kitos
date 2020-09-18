using Core.DomainModel.Advice;
using System.Linq;

namespace Core.ApplicationServices
{
    public interface IAdviceService
    {
        bool SendAdvice(int id);
        IQueryable<Advice> GetAdvicesForOrg(int orgKey);
    }
}
