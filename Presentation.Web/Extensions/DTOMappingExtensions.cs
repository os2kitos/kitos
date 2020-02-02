using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models;

namespace Presentation.Web.Extensions
{
    public static class DTOMappingExtensions
    {
        public static NamedEntityDTO MapToNamedEntityDTO<T>(this T source)
            where T : Entity, IHasName
        {
            return new NamedEntityDTO(source.Id, source.Name);
        }

        public static NamedEntityDTO MapToNamedEntityDTO(this NamedEntity source)
        {
            return new NamedEntityDTO(source.Id, source.Name);
        }

        public static NamedEntityDTO MapToNamedEntityDTO(this ItSystemUsage source)
        {
            return new NamedEntityDTO(source.Id, source.ItSystem.Name);
        }
    }
}