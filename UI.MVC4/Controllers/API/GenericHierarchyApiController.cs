using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public abstract class GenericHierarchyApiController<TModel, TDto> : GenericApiController<TModel, TDto>
        where TModel : Entity, IHierarchy<TModel>
    {
        protected GenericHierarchyApiController(IGenericRepository<TModel> repository) 
            : base(repository)
        {
        }

        public virtual HttpResponseMessage GetRoots(bool? roots, [FromUri] PagingModel<TModel> paging)
        {
            paging.Where(x => x.ParentId == null);

            return base.GetAll(paging);
        }

        public virtual HttpResponseMessage GetChildren(int id, bool? children, [FromUri] PagingModel<TModel> paging)
        {
            paging.Where(x => x.ParentId == id);

            return base.GetAll(paging);
        }
    }

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
