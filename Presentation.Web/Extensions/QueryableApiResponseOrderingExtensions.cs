using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Extensions
{
    public static class QueryableApiResponseOrderingExtensions
    {
        public static IQueryable<User> OrderUserApiResults(this IQueryable<User> src, CommonOrderByProperty? property = null)
        {
            var orderByProperty = property ?? CommonOrderByProperty.CreationOrder;

            return orderByProperty == CommonOrderByProperty.Name
                ? src.OrderBy(SelectName<User>()).ThenBy(SelectUserLastName())
                : src.OrderUnNamedApiResults(orderByProperty);
        }

        public static IQueryable<ItSystemUsage> OrderSystemUsageApiResultsByDefaultConventions(
            this IQueryable<ItSystemUsage> src,
            bool srcIsFilteredGtEqLastChanged = false,
            CommonOrderByProperty? additionalOrdering = null)
        {
            var properties = GetOrderByProperties(srcIsFilteredGtEqLastChanged, additionalOrdering);
            return src.OrderApiResults(properties, OrderSystemUsageApiResults, ThenBySystemUsageApiResults);
        }

        public static IOrderedQueryable<ItSystemUsage> OrderSystemUsageApiResults(this IQueryable<ItSystemUsage> src, CommonOrderByProperty? property = null)
        {
            var orderByProperty = property ?? CommonOrderByProperty.CreationOrder;

            return orderByProperty == CommonOrderByProperty.Name
                ? src.OrderBy(SelectSystemUsageName())
                : src.OrderUnNamedApiResults(orderByProperty);
        }

        public static IOrderedQueryable<ItSystemUsage> ThenBySystemUsageApiResults(this IOrderedQueryable<ItSystemUsage> src, CommonOrderByProperty? property)
        {
            var orderByProperty = property ?? CommonOrderByProperty.CreationOrder;

            return orderByProperty == CommonOrderByProperty.Name
                ? src.ThenBy(SelectSystemUsageName())
                : src.ThenByUnNamedApiResults(orderByProperty);
        }

        public static IQueryable<T> OrderApiResultsByDefaultConventions<T>(
            this IQueryable<T> src,
            bool srcIsFilteredGtEqLastChanged = false,
            CommonOrderByProperty? additionalOrdering = null) where T : class, IEntity, IHasName
        {
            var properties = GetOrderByProperties(srcIsFilteredGtEqLastChanged, additionalOrdering);
            return src.OrderApiResults(properties, OrderApiResults, ThenByApiResults);
        }

        public static IOrderedQueryable<T> OrderApiResults<T>(this IQueryable<T> src, CommonOrderByProperty? property) where T : class, IEntity, IHasName
        {
            var orderByProperty = property ?? CommonOrderByProperty.CreationOrder;

            return orderByProperty == CommonOrderByProperty.Name
                ? src.OrderBy(SelectName<T>())
                : src.OrderUnNamedApiResults(orderByProperty);
        }

        public static IOrderedQueryable<T> OrderUnNamedApiResults<T>(this IQueryable<T> src, CommonOrderByProperty? property) where T : class, IEntity
        {
            return (property ?? CommonOrderByProperty.CreationOrder) switch
            {
                CommonOrderByProperty.CreationOrder => src.OrderBy(SelectId<T>()),
                CommonOrderByProperty.LastChanged => src.OrderBy(SelectLastChanged<T>()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static IOrderedQueryable<T> ThenByApiResults<T>(this IOrderedQueryable<T> src, CommonOrderByProperty? property) where T : class, IEntity, IHasName
        {
            var thenByProperty = (property ?? CommonOrderByProperty.CreationOrder);

            return thenByProperty == CommonOrderByProperty.Name
                ? src.ThenBy(SelectName<T>())
                : src.ThenByUnNamedApiResults(property);
        }

        public static IOrderedQueryable<T> ThenByUnNamedApiResults<T>(this IOrderedQueryable<T> src, CommonOrderByProperty? property) where T : class, IEntity
        {
            return (property ?? CommonOrderByProperty.CreationOrder) switch
            {
                CommonOrderByProperty.CreationOrder => src.ThenBy(SelectId<T>()),
                CommonOrderByProperty.LastChanged => src.ThenBy(SelectLastChanged<T>()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static Expression<Func<T, int>> SelectId<T>() where T : class, IHasId
        {
            return x => x.Id;
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

        private static Expression<Func<User, string>> SelectUserLastName()
        {
            return x => x.LastName;
        }

        private static IEnumerable<CommonOrderByProperty> GetOrderByProperties(bool srcIsFilteredGtEqLastChanged, CommonOrderByProperty? additionalOrdering)
        {
            var properties = new List<CommonOrderByProperty>();
            if (srcIsFilteredGtEqLastChanged)
            {
                properties.Add(CommonOrderByProperty.LastChanged);
            }

            properties.Add(additionalOrdering ?? CommonOrderByProperty.CreationOrder);
            return properties;
        }

        private static IOrderedQueryable<T> OrderApiResults<T>(
            this IQueryable<T> src,
            IEnumerable<CommonOrderByProperty> properties,
            Func<IQueryable<T>, CommonOrderByProperty?, IOrderedQueryable<T>> orderBy,
            Func<IOrderedQueryable<T>, CommonOrderByProperty?, IOrderedQueryable<T>> thenBy)
        {
            return properties
                .MatchHeadAndTail()
                .Match((headAndTail) =>
                    {
                        var (head, tail) = headAndTail;
                        var initialResult = orderBy(src, head);
                        return tail.Aggregate(initialResult, (acc, thenByProperty) => thenBy(acc, thenByProperty));
                    }
                    , () => throw new ArgumentException($"{nameof(properties)} cannot be empty")
                );
        }
    }
}