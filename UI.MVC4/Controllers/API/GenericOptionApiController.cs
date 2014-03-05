using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public abstract class GenericOptionApiController<TModel, TReference> : GenericApiController<TModel, int, OptionDTO>
        where TModel : class, IOptionEntity<TReference>
    {
        protected GenericOptionApiController(IGenericRepository<TModel> repository) : base(repository)
        {
        }

        protected override IEnumerable<TModel> GetAllQuery()
        {
            return this.Repository.Get(t => t.IsActive && !t.IsSuggestion);
        }

        private IEnumerable<TModel> GetAllAllQuery()
        {
            return base.GetAllQuery();
        }


        public HttpResponseMessage GetAllAll(bool all)
        {
            if (!all) return base.GetAll();

            var items = GetAllAllQuery().ToList();

            if (!items.Any())
                return Request.CreateResponse(HttpStatusCode.NoContent);

            return Request.CreateResponse(HttpStatusCode.OK, Map<IEnumerable<TModel>, IEnumerable<OptionDTO>>(items));
        }

        protected override TModel PostQuery(TModel item)
        {
            if(!item.IsSuggestion && !User.IsInRole("GlobalAdmin"))
                throw new SecurityException();

            return base.PostQuery(item);
        }

        protected override TModel PutQuery(TModel item)
        {
            if (!item.IsSuggestion && !User.IsInRole("GlobalAdmin"))
                throw new SecurityException();

            return base.PutQuery(item);
        }

        //TODO?
        protected override TModel PatchQuery(TModel item)
        {
            return base.PatchQuery(item);
        }

        //TODO?
        protected override void DeleteQuery(int id)
        {
            base.DeleteQuery(id);
        }
    }
}
