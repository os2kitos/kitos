using System;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
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

        public override HttpResponseMessage Patch(int id, JObject obj)
        {
            try
            {
                var item = Repository.GetByKey(id);

                JToken jtoken;
                if (obj.TryGetValue("parentId", out jtoken))
                {
                    var parentId = jtoken.Value<int?>();

                    // check for circular references
                    CheckForHierarchyLoop(item, parentId);
                }

            }
            catch (Exception e)
            {
                return Error(e.Message);
            }
            return base.Patch(id, obj);
        }

        private void CheckForHierarchyLoop(TModel item, int? newParentId)
        {
            if (newParentId == null) return;
            
            // the parent-to-be of item
            var parent = Repository.GetByKey(newParentId);

            do
            {
                // did we find a loop?
                if (parent.Id == item.Id) 
                    throw new Exception("Self reference detected");

                // otherwise, check next parent
                parent = parent.Parent;
            } while (parent != null);
        }
    }
}
