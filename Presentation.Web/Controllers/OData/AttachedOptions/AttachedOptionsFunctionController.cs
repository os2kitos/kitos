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

        public virtual IHttpActionResult GetOptionsByObjectIDAndType(int id, EntityType entitytype, OptionType optiontype)
        {
            if (UserId == 0)
                return Unauthorized();

            var globalOptionData = _OptionRepository.AsQueryable().Where(s => s.IsEnabled || (s.IsEnabled && s.IsObligatory));
            var localpersonalData = _LocalOptionRepository.AsQueryable().Where(p => p.IsActive).ToList();

            List<TOption> result = new List<TOption>();
            result.AddRange(globalOptionData.AsQueryable().Where(s => s.IsObligatory));

            foreach (var p in localpersonalData)
            {
                var data = globalOptionData.AsQueryable().FirstOrDefault(s => s.Id == p.OptionId && (s.IsEnabled && !s.IsObligatory));
                if (data != null)
                {
                    result.Add(data);
                }
            }

            var options = GetAttachedOptions(optiontype, id, entitytype);

            if (options != null)
            {
                if (options.Count() <= 0)
                {
                    return Ok(result);
                }
                foreach (var o in options)
                {
                    var currentOption = result.FirstOrDefault(r => r.Id == o.OptionId);
                    if (currentOption != null)
                    {
                        result.FirstOrDefault(r => r.Id == o.OptionId).Checked = true;
                    }
                    else
                    {
                        _AttachedOptionRepository.Delete(o);
                        _AttachedOptionRepository.Save();
                    }
                }
            }
            else
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return Ok(result);
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

        private List<AttachedOption> GetAttachedOptions(OptionType type, int id, EntityType objectType)
        {
                var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(AttachedOption));

                if (_authService.HasReadAccessOutsideContext(UserId) || hasOrg == false)
                {
                    //tolist so we can operate with open datareaders in the following foreach loop.
                    return _AttachedOptionRepository.AsQueryable().Where(x => x.ObjectId == id
                    && x.OptionType == type
                    && x.ObjectType == objectType).ToList();
                }
                else
                {
                    return _AttachedOptionRepository.AsQueryable()
                     .Where(x => ((IHasOrganization)x).OrganizationId == _authService.GetCurrentOrganizationId(UserId) 
                     && x.ObjectId == id 
                     && x.OptionType == type
                     && x.ObjectType == objectType).ToList();
                }
            }
    }


}