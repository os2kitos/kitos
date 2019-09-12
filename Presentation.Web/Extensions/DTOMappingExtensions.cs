using Core.DomainModel;
using Presentation.Web.Models;

namespace Presentation.Web.Extensions
{
    public static class DTOMappingExtensions
    {
        public static NamedEntityDTO MapToNamedEntityDTO<T>(this T result)
            where T : Entity, IHasName
        {
            return new NamedEntityDTO() {Id = result.Id, Name = result.Name};
        }
    }
}