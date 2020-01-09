using System;
using System.Linq;
using System.Net.Http;
using System.Security;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [MigratedToNewAuthorizationContext]
    public abstract class GenericOptionApiController<TModel, TReference, TDto> : GenericApiController<TModel, TDto>
        where TModel : OptionEntity<TReference>
    {
        protected GenericOptionApiController(
            IGenericRepository<TModel> repository, 
            IAuthorizationContext authorizationContext = null) //TODO: Do not allow null once completed
            : base(repository, authorizationContext)
        {
        }

        protected override IQueryable<TModel> GetAllQuery()
        {
            return Repository.AsQueryable().Where(t => t.IsLocallyAvailable && !t.IsSuggestion);
        }

        public HttpResponseMessage GetAllSuggestions(bool? suggestions)
        {
            try
            {
                var items = Repository
                    .AsQueryable()
                    .Where(t => t.IsSuggestion)
                    .AsEnumerable()
                    .Where(AllowRead)
                    .Select(Map);

                return Ok(items);
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
                var items = Repository
                    .AsQueryable()
                    .Where(t => !t.IsSuggestion)
                    .AsEnumerable()
                    .Where(AllowRead)
                    .Select(Map);

                return Ok(items);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override TModel PutQuery(TModel item)
        {
            if (!item.IsSuggestion && !AllowModify(item))
                throw new SecurityException();

            return base.PutQuery(item);
        }
    }
}
