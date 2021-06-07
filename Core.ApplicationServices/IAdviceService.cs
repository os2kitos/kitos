using System.Collections.Generic;
using Core.DomainModel.Advice;
using System.Linq;

namespace Core.ApplicationServices
{
    public interface IAdviceService
    {
        void CreateAdvice(Advice advice);
        IQueryable<Advice> GetAdvicesForOrg(int orgKey);
        IQueryable<Advice> GetAdvicesAccessibleToCurrentUser();
        bool SendAdvice(int id);
        void UpdateSchedule(Advice advice);
        void Delete(Advice key);
        void BulkDeleteAdvice(IEnumerable<Advice> toBeDeleted);
        void Deactivate(Advice advice);
    }
}
