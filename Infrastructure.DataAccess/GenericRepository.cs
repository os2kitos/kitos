using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
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
        private bool _disposed;

        public GenericRepository(KitosContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> Get(
           Expression<Func<T, bool>> filter = null,
           Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
           string includeProperties = "",
           bool readOnly = false)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }


            query = orderBy?.Invoke(query) ?? query;
            if (readOnly)
            {
                query = query.AsNoTracking();
            }

            return query.ToList();
        }

        public IQueryable<T> AsQueryable(bool readOnly)
        {
            var dbAsQueryable = _dbSet.AsQueryable();
            if (readOnly)
            {
                dbAsQueryable = dbAsQueryable.AsNoTracking();
            }
            return dbAsQueryable;
        }

        public T Create()
        {
            return _dbSet.Create<T>();
        }

        public T GetByKey(params object[] key)
        {
            return _dbSet.Find(key);
        }
       

        public T Insert(T entity)
        {
            return _dbSet.Add(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteByKey(params object[] key)
        {
            var entityToDelete = _dbSet.Find(key);
            _dbSet.Remove(entityToDelete);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Patch(T entity)
        {
            _context.Configuration.ValidateOnSaveEnabled = false;
            _dbSet.Attach(entity);
            var entry = _context.Entry(entity);
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                if (propertyInfo.Name == "Id")
                    continue; // skip primary key

                if (propertyInfo.GetValue(entity) != null)
                    entry.Property(propertyInfo.Name).IsModified = true;
            }
        }

        public IEnumerable<T> SQL(string sql)
        {
            return _dbSet.SqlQuery(sql);
        }

        public void Save()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                throw;
            }
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