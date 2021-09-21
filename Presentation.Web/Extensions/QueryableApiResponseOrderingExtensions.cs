using System.Linq;
using Core.DomainModel;

namespace Presentation.Web.Extensions
{
    public static class QueryableApiResponseOrderingExtensions
    {
        public static IQueryable<T> OrderByDefaultConventions<T>(this IQueryable<T> src, bool srcIsFilteredGtEqLastChanged) where T : class, IEntity
        {
            return srcIsFilteredGtEqLastChanged
                ? src.OrderBy(x => x.LastChanged).ThenBy(x => x.Id)
                : src.OrderBy(x => x.Id);
        }
    }
}