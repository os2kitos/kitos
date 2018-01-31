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
using System.Web.Mvc;
using System.Web.OData;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
        public class AttachedOptionsController : BaseEntityController<AttachedOption>
        {
            IAuthenticationService _authService;
            IGenericRepository<AttachedOption> _optionRepository;
            IGenericRepository<ItSystem> _itSystemRepository;


            public AttachedOptionsController(IGenericRepository<AttachedOption> repository, IAuthenticationService authService, IGenericRepository<ItSystem> itSystemRepository)
               : base(repository, authService)
            {
                _authService = authService;
                _optionRepository = repository;
                _itSystemRepository = itSystemRepository;

            }

            [System.Web.Http.HttpDelete]
            [EnableQuery]
            [ODataRoute("RemoveOption(id={id}, objectId={objectId}, type={type})")]
            public IHttpActionResult RemoveOption(int id, int objectId, OptionType type)
            {
                var option = _optionRepository.AsQueryable().FirstOrDefault(o => o.OptionId == id && o.ObjectId == objectId && o.OptionType == type);

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

            public virtual List<AttachedOption> GetAttachedOptions(OptionType type, int id)
            {
                var system = _itSystemRepository.AsQueryable().FirstOrDefault(s => s.Id == id);

                if (system != null)
                {
                    var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(AttachedOption));

                    if (_authService.HasReadAccessOutsideContext(UserId) || hasOrg == false)
                    {
                        //tolist so we can operate with open datareaders in the following foreach loop.
                        return _optionRepository.AsQueryable().Where(x => x.ObjectId == system.Id && x.OptionType == type).ToList();
                    }
                    else
                    {
                        return _optionRepository.AsQueryable()
                         .Where(x => ((IHasOrganization)x).OrganizationId == _authService.GetCurrentOrganizationId(UserId) && x.ObjectId == id && x.OptionType == type).ToList();
                    }
                }
                return null;
            }
        }

    
}