using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public abstract class GenericOptionApiController<TModel, TReference, TDto> : GenericApiController<TModel, int, TDto>
        where TModel : class, IOptionEntity<TReference>
    {
        protected GenericOptionApiController(IGenericRepository<TModel> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<TModel> GetAllQuery()
        {
            return this.Repository.Get(t => t.IsActive && !t.IsSuggestion);
        }

        public HttpResponseMessage GetAllSuggestions(bool? suggestions)
        {
            var items = this.Repository.Get(t => t.IsSuggestion);
            
            return Ok(Map<IEnumerable<TModel>, IEnumerable<TDto>>(items));
        }

        public HttpResponseMessage GetAllNonSuggestions(bool? nonsuggestions)
        {
            var items = this.Repository.Get(t => !t.IsSuggestion);

            return Ok(Map<IEnumerable<TModel>, IEnumerable<TDto>>(items));
        }
        
        protected override TModel PutQuery(TModel item)
        {
            if (!item.IsSuggestion && !IsGlobalAdmin())
                throw new SecurityException();

            return base.PutQuery(item);
        }
    }
}
