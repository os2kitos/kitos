using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UI.MVC4.Models
{
    public class PagingModel<TModel>
    {
        public PagingModel()
        {
            _filters = new List<Expression<Func<TModel, bool>>>();
            Skip = 0;
            Take = 100;
            OrderBy = "Id";
            Descending = false;
        }

        public int Take { get; set; }
        public int Skip { get; set; }
        public string OrderBy { get; set; }
        public bool Descending { get; set; }

        private readonly List<Expression<Func<TModel, bool>>> _filters;

        public PagingModel<TModel> Where(Expression<Func<TModel, bool>> filter)
        {
            _filters.Add(filter);
            return this;
        }

        public IQueryable<TModel> Filter(IQueryable<TModel> query)
        {
            foreach (var filter in _filters)
            {
                query = query.Where(filter);
            }

            return query;
        }
    }
}