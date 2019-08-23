using System;
using System.Linq;
using System.Net.Http;
using System.Security;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Authorization;
using Presentation.Web.Infrastructure.Authorization.Context;

namespace Presentation.Web.Controllers.API
{
    public abstract class GenericOptionApiController<TModel, TReference, TDto> : GenericApiController<TModel, TDto>
        where TModel : OptionEntity<TReference>
    {
        protected GenericOptionApiController(IGenericRepository<TModel> repository, IAuthorizationContext authorizationContext = null)
            : base(repository, authorizationContext)
        {
        }

        protected override IQueryable<TModel> GetAllQuery()
        {
            return Repository.AsQueryable(readOnly:true).Where(t => t.IsLocallyAvailable && !t.IsSuggestion);
        }

        public HttpResponseMessage GetAllSuggestions(bool? suggestions)
        {
            try
            {
                var items = Repository.AsQueryable(readOnly:true).Where(t => t.IsSuggestion);

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
                var items = Repository.AsQueryable(readOnly:true).Where(t => !t.IsSuggestion);

                return Ok(Map(items));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override TModel PutQuery(TModel item)
        {
            if (!item.IsSuggestion && !AllowWriteAccess(item)) 
                throw new SecurityException();

            return base.PutQuery(item);
        }
    }
}
