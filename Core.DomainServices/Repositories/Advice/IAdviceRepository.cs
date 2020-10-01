using System.Linq;

namespace Core.DomainServices.Repositories.Advice
{
    public interface IAdviceRepository
    {
        IQueryable<DomainModel.Advice.Advice> GetOrphans();
        void Delete(DomainModel.Advice.Advice advice);
    }
}
