using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Presentation.Web.Models.API.V1
{
    public class PagingModel<TModel>
    {
        public PagingModel()
        {
            _dbFilters = new List<Expression<Func<TModel, bool>>>();
            Skip = 0;
            Take = 100;
            OrderBy = "Id";
            Descending = false;
        }

        /// <summary>
        /// Størrelse på resultatsættet.
        /// Standardværdien er '100'
        /// </summary>
        public int Take { get; set; }
        /// <summary>
        /// Antal der skal ignoreres inden resultatsættet dannes.
        /// Standardværdien er '0'
        /// </summary>
        public int Skip { get; set; }
        /// <summary>
        /// Bestemmer hvilket felt der sorteres på inden resultatsættet dannes.
        /// Standardværdien er 'Id'
        /// </summary>
        public string OrderBy { get; set; }
        /// <summary>
        /// Bestemmer om sorteringen skal være faldende
        /// Standardværdien er 'false'
        /// </summary>
        public bool Descending { get; set; }

        private readonly List<Expression<Func<TModel, bool>>> _dbFilters;

        public PagingModel<TModel> Where(Expression<Func<TModel, bool>> filter)
        {
            _dbFilters.Add(filter);
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
    }
}
