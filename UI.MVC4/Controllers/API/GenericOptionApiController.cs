using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public abstract class GenericOptionApiController<TModel, TReference, TDto> : GenericApiController<TModel, TDto>
        where TModel : Entity, IOptionEntity<TReference>
    {
        protected GenericOptionApiController(IGenericRepository<TModel> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<TModel> GetAllQuery(int skip, int take, bool descending, string orderBy = null)
        {
            var field = orderBy ?? "Id";
            return Repository.AsQueryable().Where(t => t.IsActive && !t.IsSuggestion).OrderByField(field, descending).Skip(skip).Take(take);
        }

        public HttpResponseMessage GetAllSuggestions(bool? suggestions, int skip = 0, int take = 100)
        {
            var items = Repository.AsQueryable().Where(t => t.IsSuggestion).OrderBy(x => x.Id).Skip(skip).Take(take);
            
            return Ok(Map(items));
        }

        public HttpResponseMessage GetAllNonSuggestions(bool? nonsuggestions, int skip = 0, int take = 100)
        {
            var items = Repository.AsQueryable().Where(t => !t.IsSuggestion).OrderBy(x => x.Id).Skip(skip).Take(take);

            return Ok(Map(items));
        }
        
        protected override TModel PutQuery(TModel item)
        {
            if (!item.IsSuggestion && !IsGlobalAdmin())
                throw new SecurityException();

            return base.PutQuery(item);
        }
    }
}
