using Core.DomainModel.Advice;

namespace Core.ApplicationServices.Jobs
{
    public interface IAdviceScheduler
    {
        void Remove(Advice advice);
    }
}