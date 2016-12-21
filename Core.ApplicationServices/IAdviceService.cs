using Core.DomainModel.Advice;

namespace Core.ApplicationServices
{
    public interface IAdviceService
    {
        bool sendAdvice(int id);
    }
}
