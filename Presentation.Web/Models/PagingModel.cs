using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Presentation.Web.Models
{
    public class PagingModel<TModel>
    {
        public PagingModel()
        {
            _dbFilters = new List<Expression<Func<TModel, bool>>>();
            _postProcessingFilters = new List<Predicate<TModel>>();
            Skip = 0;
            Take = 100;
            OrderBy = "Id";
            Descending = false;
        }

        /// <summary>
        /// Page size.
        /// Default value is 100
        /// </summary>
        public int Take { get; set; }
        /// <summary>
        /// Items to skip before taking the ''page size'' of items.
        /// Default value is '0'
        /// </summary>
        public int Skip { get; set; }
        /// <summary>
        /// Determine by which field the result set should be ordered before paging is applied.
        /// Default value is 'Id'
        /// </summary>
        public string OrderBy { get; set; }
        /// <summary>
        /// Determines if ordering should be descending with respect to 'OrderBy'.
        /// Default value is 'false'
        /// </summary>
        public bool Descending { get; set; }

        private readonly List<Expression<Func<TModel, bool>>> _dbFilters;
        private readonly List<Predicate<TModel>> _postProcessingFilters;

        public PagingModel<TModel> Where(Expression<Func<TModel, bool>> filter)
        {
            _dbFilters.Add(filter);
            return this;
        }

        /// <summary>
        /// Add post-processing filter, which can be applied to in-memory objects before applying the paging.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public PagingModel<TModel> WithPostProcessingFilter(Predicate<TModel> filter)
        {
            _postProcessingFilters.Add(filter);
            return this;
        }

        public IQueryable<TModel> Filter(IQueryable<TModel> query)
        {
            foreach (var filter in _dbFilters)
            {
                query = query.Where(filter);
            }

            return query;
        }

        /// <summary>
        /// Applies pre-paging processing of data queried by main filters.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public IQueryable<TModel> ApplyPostProcessing(IQueryable<TModel> content)
        {
            if (_postProcessingFilters.Any())
            {
                return content
                    .AsEnumerable()
                    .Where(x => _postProcessingFilters.ToList().Any(filter => filter(x) == false) == false)
                    .AsQueryable();
            }

            return content;
        }
    }
}
