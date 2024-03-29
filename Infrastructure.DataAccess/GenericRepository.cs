﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Core.DomainServices;
using Infrastructure.DataAccess.Extensions;

namespace Infrastructure.DataAccess
{
    public class GenericRepository<T> : IGenericRepository<T>
        where T : class
    {
        private readonly KitosContext _context;
        private readonly DbSet<T> _dbSet;
        private bool _disposed;

        private readonly Lazy<EntityPropertyProxyValueLoader<T>> _proxyValueLoader = new(() => new EntityPropertyProxyValueLoader<T>());

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
                (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }


            query = orderBy?.Invoke(query) ?? query;

            return query.ToList();
        }

        public IQueryable<T> GetWithReferencePreload<TProperty>(Expression<Func<T, TProperty>> includeExpression)
        {
            return _dbSet.Include(includeExpression);
        }

        public TProperty GetMax<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            return _dbSet.Max(propertyExpression);
        }

        public IQueryable<T> AsQueryable()
        {
            var dbAsQueryable = _dbSet.AsQueryable();

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

        public void AddRange(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            if (entities.Any())
            {
                _dbSet.RemoveRange(entities);
            }
        }

        public void DeleteWithReferencePreload(T entity)
        {
            LoadReferencedEntities(entity);
            Delete(entity);
        }

        public void DeleteByKey(params object[] key)
        {
            var entityToDelete = _dbSet.Find(key);
            if (entityToDelete != null)
            {
                _dbSet.Remove(entityToDelete);
            }
        }

        public void DeleteByKeyWithReferencePreload(params object[] key)
        {
            var entityToDelete = _dbSet.Find(key);
            if (entityToDelete == null)
            {
                throw new ArgumentException(
                    $"Unable to locate entity of type {typeof(T).Namespace} from the provided key {string.Join(";", key.Select(k => k.ToString()))}");
            }
            LoadReferencedEntities(entityToDelete);

            Delete(entityToDelete);
        }

        private void LoadReferencedEntities(T entity)
        {
            _proxyValueLoader.Value.LoadReferencedEntities(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
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

        public int Count => _dbSet.Count();

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