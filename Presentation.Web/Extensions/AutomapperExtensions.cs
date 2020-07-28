using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Core.DomainModel;

namespace Presentation.Web.Extensions
{
    public static class AutoMapperExtensions
    {
        public static IMappingExpression<TSource, TDestination> IgnoreDestinationValueFor<TSource, TDestination, TMember>(this IMappingExpression<TSource, TDestination> expr, Expression<Func<TDestination, TMember>> pickMember)
            where TDestination : IEntity
        {
            return expr
                .ForMember(
                    destinationMember: pickMember,
                    memberOptions: options => options.Ignore()
                );
        }

        /// <summary>
        /// Ensures that IEntity fields are not automatically created by AutoMapper when an input DTO is provided
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> IgnoreDestinationEntityFields<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expr)
            where TDestination : IEntity
        {
            return typeof(TDestination)
                .GetProperties()
                .Where(property => typeof(IEntity).IsAssignableFrom(property.PropertyType))
                .Select(x => x.Name)
                .Aggregate
                (
                    seed: expr,
                    func: (acc, next) => 
                        acc.ForMember(next, options => options.Ignore())
                );
        }

        /// <summary>
        /// Ensures that virtual collections are automatically created by AutoMapper when an input DTO is provided
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> IgnoreVirtualCollectionFields<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expr)
            where TDestination : IEntity
        {
            return typeof(TDestination)
                .GetProperties()
                .Where(property => typeof(IEntity).IsAssignableFrom(property.PropertyType))
                .Select(x => x.Name)
                .Aggregate
                (
                    seed: expr,
                    func: (acc, next) =>
                        acc.ForMember(next, options => options.Ignore())
                );
        }
    }
}