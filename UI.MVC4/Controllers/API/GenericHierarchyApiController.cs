using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

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
}
