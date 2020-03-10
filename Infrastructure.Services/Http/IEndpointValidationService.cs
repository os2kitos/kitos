using System.Threading.Tasks;

namespace Infrastructure.Services.Http
{
    public interface IEndpointValidationService
    {
        Task<EndpointValidation> ValidateAsync(string url);
    }
}
