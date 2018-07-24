using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.OData;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AttachedOptionsFunctionController<TEntity, TOption, TLocalOption> : AttachedOptionsController
    where TEntity : Entity
    where TOption : OptionHasChecked<TEntity>
    where TLocalOption : LocalOptionEntity<TOption>
    {
        IGenericRepository<AttachedOption> _AttachedOptionRepository;
        IAuthenticationService _authService;
        IGenericRepository<TOption> _OptionRepository;
        IGenericRepository<TLocalOption> _LocalOptionRepository;

        public AttachedOptionsFunctionController(IGenericRepository<AttachedOption> repository, IAuthenticationService authService,
            IGenericRepository<TOption> OptionRepository,
            IGenericRepository<TLocalOption> LocalOptionRepository)
               : base(repository, authService)
        {
            _authService = authService;
            _AttachedOptionRepository = repository;
            _OptionRepository = OptionRepository;
            _LocalOptionRepository = LocalOptionRepository;
        }
        
        [System.Web.Http.HttpDelete]
        [EnableQuery]
        [ODataRoute("RemoveOption(id={id}, objectId={objectId}, type={type}, entityType={entityType})")]
        public IHttpActionResult RemoveOption(int id, int objectId, OptionType type, EntityType entityType)
        {
            var option = _AttachedOptionRepository.AsQueryable().FirstOrDefault(o => o.OptionId == id 
            && o.ObjectId == objectId 
            && o.OptionType == type
            && o.ObjectType == entityType);

            if (option == null)
                return NotFound();

            if (!_authService.HasWriteAccess(UserId, option))
                return Unauthorized();

            try
            {
                Repository.DeleteByKey(option.Id);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}