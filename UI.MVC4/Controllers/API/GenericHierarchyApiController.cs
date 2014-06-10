using System;
using System.Linq;
using System.Net.Http;
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

        public virtual HttpResponseMessage GetRoots(bool? roots, int skip = 0, int take = 100, string orderBy = null, bool descending = false)
        {
            try
            {
                var query = Repository.AsQueryable().Where(x => x.ParentId == null);
                var totalCount = query.Count();
                // TODO there needs to be a more generic way to make these queries as this is c/p from GetAllQuery()
                var field = orderBy ?? "Id";
                var page = query.OrderByField(field, descending).Skip(skip).Take(take);

                var paginationHeader = new { TotalCount = totalCount };
                System.Web.HttpContext.Current.Response.Headers.Add("X-Pagination",
                                                                    Newtonsoft.Json.JsonConvert.SerializeObject(
                                                                        paginationHeader));

                return Ok(Map(page.Cast<TModel>()));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public virtual HttpResponseMessage GetChildren(int id, bool? children, int skip = 0, int take = 100, string orderBy = null, bool descending = false)
        {
            try
            {
                var item = Repository.GetByKey(id);

                if (item == null) return NotFound();

                // TODO there needs to be a more generic way to make these queries as this is c/p from GetAllQuery()
                var query = item.Children.AsQueryable();
                var totalCount = query.Count();
                var field = orderBy ?? "Id";
                var page = query.OrderByField(field, descending).Skip(skip).Take(take);

                var paginationHeader = new { TotalCount = totalCount };
                System.Web.HttpContext.Current.Response.Headers.Add("X-Pagination",
                                                                    Newtonsoft.Json.JsonConvert.SerializeObject(
                                                                        paginationHeader));

                return Ok(Map(page));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
