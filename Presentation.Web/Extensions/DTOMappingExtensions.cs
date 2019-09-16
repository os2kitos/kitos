using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;
using Presentation.Web.Models;

namespace Presentation.Web.Extensions
{
    public static class DTOMappingExtensions
    {
        public static NamedEntityDTO MapToNamedEntityDTO<T>(this T result)
            where T : Entity, IHasName
        {
            return new NamedEntityDTO { Id = result.Id, Name = result.Name };
        }

        public static NamedEntityDTO MapToNamedEntityDTO(this NamedEntity result)
        {
            return new NamedEntityDTO { Id = result.Id, Name = result.Name };
        }
    }
}