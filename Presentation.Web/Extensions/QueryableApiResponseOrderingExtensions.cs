using System;
using System.Linq;
using System.Linq.Expressions;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Extensions
{
    public static class QueryableApiResponseOrderingExtensions
    {
        public static IQueryable<ItSystemUsage> OrderSystemUsageByDefaultConventions(
            this IQueryable<ItSystemUsage> src,
            bool srcIsFilteredGtEqLastChanged = false,
            CommonOrderByProperty? additionalOrdering = null)
        {
            return srcIsFilteredGtEqLastChanged
                ? src
                    .OrderSystemUsageResults(CommonOrderByProperty.LastChanged)
                    .ThenBySystemUsageResults(additionalOrdering)
                : src.OrderSystemUsageResults(additionalOrdering);
        }

        public static IOrderedQueryable<ItSystemUsage> OrderSystemUsageResults(this IQueryable<ItSystemUsage> src, CommonOrderByProperty? property)
        {
            var orderByProperty = property ?? CommonOrderByProperty.Id;

            return orderByProperty == CommonOrderByProperty.Name
                ? src.OrderBy(SelectSystemUsageName())
                : src.OrderUnNamedResults(orderByProperty);
        }

        public static IOrderedQueryable<ItSystemUsage> ThenBySystemUsageResults(this IOrderedQueryable<ItSystemUsage> src, CommonOrderByProperty? property)
        {
            var orderByProperty = property ?? CommonOrderByProperty.Id;

            return orderByProperty == CommonOrderByProperty.Name
                ? src.ThenBy(SelectSystemUsageName())
                : src.ThenByUnNamedResults(orderByProperty);
        }

        public static IOrderedQueryable<T> OrderByDefaultConventions<T>(
            this IQueryable<T> src,
            bool srcIsFilteredGtEqLastChanged = false,
            CommonOrderByProperty? additionalOrdering = null) where T : class, IEntity, IHasName
        {
            return srcIsFilteredGtEqLastChanged
                ? src
                    .OrderResults(CommonOrderByProperty.LastChanged)
                    .ThenByResults(additionalOrdering)
                : src.OrderResults(additionalOrdering);
        }


        public static IOrderedQueryable<T> OrderResults<T>(this IQueryable<T> src, CommonOrderByProperty? property) where T : class, IEntity, IHasName
        {
            var orderByProperty = property ?? CommonOrderByProperty.Id;
            
            return orderByProperty == CommonOrderByProperty.Name
                ? src.OrderBy(SelectName<T>())
                : src.OrderUnNamedResults(orderByProperty);
        }

        public static IOrderedQueryable<T> OrderUnNamedResults<T>(this IQueryable<T> src, CommonOrderByProperty? property) where T : class, IEntity
        {
            return (property ?? CommonOrderByProperty.Id) switch
            {
                CommonOrderByProperty.Id => src.OrderBy(SelectId<T>()),
                CommonOrderByProperty.LastChanged => src.OrderBy(SelectLastChanged<T>()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static IOrderedQueryable<T> ThenByResults<T>(this IOrderedQueryable<T> src, CommonOrderByProperty? property) where T : class, IEntity, IHasName
        {
            var thenByProperty = (property ?? CommonOrderByProperty.Id);
            
            return thenByProperty == CommonOrderByProperty.Name
                ? src.ThenBy(SelectName<T>())
                : src.ThenByUnNamedResults(property);
        }

        public static IOrderedQueryable<T> ThenByUnNamedResults<T>(this IOrderedQueryable<T> src, CommonOrderByProperty? property) where T : class, IEntity
        {
            return (property ?? CommonOrderByProperty.Id) switch
            {
                CommonOrderByProperty.Id => src.ThenBy(SelectId<T>()),
                CommonOrderByProperty.LastChanged => src.ThenBy(SelectLastChanged<T>()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static Expression<Func<T, int>> SelectId<T>() where T : class, IHasId
        {
            return x=>x.Id;
        }
        private static Expression<Func<T, string>> SelectName<T>() where T : class, IHasName
        {
            return x => x.Name;
        }

        private static Expression<Func<T, DateTime>> SelectLastChanged<T>() where T : class, IEntity
        {
            return x => x.LastChanged;
        }

        private static Expression<Func<ItSystemUsage, string>> SelectSystemUsageName()
        {
            return x => x.ItSystem.Name;
        }
    }
}