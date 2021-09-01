using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Core.DomainServices
{
    public interface IGenericRepository<TModel> : IDisposable
        where TModel : class
    {
        IEnumerable<TModel> Get(
            Expression<Func<TModel, bool>> filter = null,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null,
            string includeProperties = "");

        IQueryable<TModel> GetWithReferencePreload<TProperty>(Expression<Func<TModel, TProperty>> includeExpression);

        TProperty GetMax<TProperty>(Expression<Func<TModel, TProperty>> propertyExpression);

        TModel GetByKey(params object[] key);

        TModel Insert(TModel entity);

        void AddRange(IEnumerable<TModel> entities);

        void RemoveRange(IEnumerable<TModel> entities);

        /// <summary>
        /// Consider using <see cref="DeleteWithReferencePreload"/> and remove any pre-delete manual loading of child refs.
        /// </summary>
        /// <param name="entity"></param>
        void Delete(TModel entity);

        /// <summary>
        /// Loads all reference fields before deleting the entity. Once all usages of <see cref="Delete"/> have been migrated to this, it will be renamed to Delete
        /// Fixes the situation where optional many-many FK are not nulled upon object deletion of the root entity, so cascade will not detect it.
        /// </summary>
        /// <param name="entity"></param>

        void DeleteWithReferencePreload(TModel entity);
        /// <summary>
        /// Loads all reference fields before deleting the entity. Once all usages of <see cref="DeleteByKey"/> have been migrated to this, it will be renamed to DeleteByKey
        /// Fixes the situation where optional many-many FK are not nulled upon object deletion of the root entity, so cascade will not detect it.
        /// </summary>

        void DeleteByKeyWithReferencePreload(params object[] key);
        /// <summary>
        /// Consider using <see cref="DeleteByKeyWithReferencePreload"/> and remove any pre-delete manual loading of child refs.
        /// </summary>
        /// <param name="entity"></param>

        void DeleteByKey(params object[] key);

        void Update(TModel entity);

        void Save();

        IQueryable<TModel> AsQueryable();

        IEnumerable<TModel> SQL(string sql);

        TModel Create();

        int Count { get; }
    }
}