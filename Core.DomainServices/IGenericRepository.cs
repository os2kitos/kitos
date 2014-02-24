using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IGenericRepository<TModel> : IDisposable
        where TModel : class
    {
        IEnumerable<TModel> Get(
            Expression<Func<TModel, bool>> filter = null,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null,
            string includeProperties = "");
        TModel GetById<TKeyType>(TKeyType id);
        void Insert(TModel entity);
        void DeleteById<TKeyType>(TKeyType id);
        void Update(TModel entity);
        void Save();
    }
}