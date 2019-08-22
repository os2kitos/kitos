using System;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Authorization;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public abstract class GenericHierarchyApiController<TModel, TDto> : GenericContextAwareApiController<TModel, TDto>
        where TModel : Entity, IHierarchy<TModel>, IContextAware
    {
        protected GenericHierarchyApiController(IGenericRepository<TModel> repository, IAuthorizationContext authorizationContext = null)
            : base(repository, authorizationContext)
        {
        }

        protected override void DeleteQuery(TModel entity)
        {
            // http://stackoverflow.com/questions/15226312/entityframewok-how-to-configure-cascade-delete-to-nullify-foreign-keys
            // when children are loaded into memory the foreign key is correctly set to null on children when deleted
            //var todo = Repository.Get(x => x.Id == entity.Id, null, "Children").FirstOrDefault();
            var dummy = entity.Children;
            Repository.DeleteByKey(entity.Id);
            Repository.Save();
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

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
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
                return LogError(e);
            }
            return base.Patch(id, organizationId, obj);
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
