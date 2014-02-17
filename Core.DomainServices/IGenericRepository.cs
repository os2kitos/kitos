using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Core.DomainServices
{
    public interface IGenericRepository<T> : IDisposable
        where T : class
    {
        IEnumerable<T> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "");
        T GetById(int id);
        void Insert(T entity);
        void DeleteById(int id);
        void Update(T entity);
        void Save();
    }
}