using Core.DomainModel.Advice;

namespace Core.ApplicationServices.Helpers
{
    public interface IHangfireHelper
    {
        void RemoveFromHangfire(Advice advice);
    }
}