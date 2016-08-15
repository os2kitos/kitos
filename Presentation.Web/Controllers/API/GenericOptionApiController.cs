using System;
using System.Linq;
using System.Net.Http;
using System.Security;
using Core.DomainModel;
using Core.DomainServices;

namespace Presentation.Web.Controllers.API
{
    public abstract class GenericOptionApiController<TModel, TReference, TDto> : GenericApiController<TModel, TDto>
        where TModel : Entity, IOptionEntity<TReference>
    {
        protected GenericOptionApiController(IGenericRepository<TModel> repository)
            : base(repository)
        {
        }

        protected override IQueryable<TModel> GetAllQuery()
        {
            return Repository.AsQueryable().Where(t => t.IsActive && !t.IsSuggestion);
        }

        public HttpResponseMessage GetAllSuggestions(bool? suggestions)
        {
            try
            {
                var items = Repository.AsQueryable().Where(t => t.IsSuggestion);

                return Ok(Map(items));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetAllNonSuggestions(bool? nonsuggestions)
        {
            try
            {
                var items = Repository.AsQueryable().Where(t => !t.IsSuggestion);

                return Ok(Map(items));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override TModel PutQuery(TModel item)
        {
            if (!item.IsSuggestion && !IsGlobalAdmin())
                throw new SecurityException();

            return base.PutQuery(item);
        }
    }
}
