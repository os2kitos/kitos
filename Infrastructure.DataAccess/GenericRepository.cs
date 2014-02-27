using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Core.DomainServices;

namespace Infrastructure.DataAccess
{
    public class GenericRepository<T> : IGenericRepository<T>
        where T : class
    {
        private readonly KitosContext _context;
        private readonly DbSet<T> _dbSet;
        private bool _disposed = false;

        public GenericRepository(KitosContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> Get(
           Expression<Func<T, bool>> filter = null,
           Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
           string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return orderBy != null ? orderBy(query).ToList() : query.ToList();
        }

        public T GetById<TKeyType>(TKeyType id)
        {
            return _dbSet.Find(id);
        }

        public T Insert(T entity)
        {
            return _dbSet.Add(entity);
        }

        public void DeleteById<TKeyType>(TKeyType id)
        {
            var entityToDelete = _dbSet.Find(id);
            _dbSet.Remove(entityToDelete);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}