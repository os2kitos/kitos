using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
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
        public class AttachedOptionsRegisterTypesController : AttachedOptionsController
    {
            IAuthenticationService _authService;
            IGenericRepository<AttachedOption> _optionRepository;
            IGenericRepository<RegisterType> _registerTypeRepository;
            IGenericRepository<LocalRegisterType> _localRegisterTypeRepository;
            IGenericRepository<ItSystemUsage> _usageRepository;
        
            public AttachedOptionsRegisterTypesController(IGenericRepository<AttachedOption> repository, IAuthenticationService authService, IGenericRepository<ItSystem> itSystemRepository,
                IGenericRepository<RegisterType> registerTypeRepository,
                IGenericRepository<LocalRegisterType> localRegisterTypeRepository,
                IGenericRepository<ItSystemUsage> usageRepository)
               : base(repository, authService, itSystemRepository)
            {
            _registerTypeRepository = registerTypeRepository;
            _localRegisterTypeRepository = localRegisterTypeRepository;
            _usageRepository = usageRepository;
            _authService = authService;
            _optionRepository = repository;
            }

            [System.Web.Http.HttpGet]
            [EnableQuery]
            [ODataRoute("GetRegisterTypesByObjectID(id={id})")]
            public IHttpActionResult GetRegisterTypeByObjectID(int id)
            {
                if (UserId == 0)
                    return Unauthorized();

                var globalOptionData = _registerTypeRepository.AsQueryable().Where(s => s.IsEnabled || (s.IsEnabled && s.IsObligatory));
                var localRegisterTypes = _localRegisterTypeRepository.AsQueryable().Where(p => p.IsActive).ToList();

                List<RegisterType> result = new List<RegisterType>();
                result.AddRange(globalOptionData.AsQueryable().Where(s => s.IsObligatory));    

                foreach (var p in localRegisterTypes)
                {
                    var data = globalOptionData.AsQueryable().FirstOrDefault(s => s.Id == p.OptionId && (s.IsEnabled && !s.IsObligatory));
                    if (data != null)
                    {
                        result.Add(data);
                    }
                }

                var options = GetAttachedOptions(OptionType.REGISTERTYPEDATA, id);

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
                            _optionRepository.Delete(o);
                            _optionRepository.Save();
                        }
                    }
                }
                else
                {
                return StatusCode(HttpStatusCode.NoContent);
                }

                return Ok(result);
            }

        //usage instead of system.
        public override List<AttachedOption> GetAttachedOptions(OptionType type, int id)
        {
             var usage = _usageRepository.AsQueryable().FirstOrDefault(s => s.Id == id);

             if (usage != null)
             {
                 var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(AttachedOption));

                 if (_authService.HasReadAccessOutsideContext(UserId) || hasOrg == false)
                 {
                     //tolist so we can operate with open datareaders in the following foreach loop.
                     return _optionRepository.AsQueryable().Where(x => x.ObjectId == usage.Id && x.OptionType == type).ToList();
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