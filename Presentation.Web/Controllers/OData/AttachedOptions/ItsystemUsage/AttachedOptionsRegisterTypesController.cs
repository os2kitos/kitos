using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.OData;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    public class AttachedOptionsRegisterTypesController : AttachedOptionsFunctionController<ItSystemUsage,RegisterType, LocalRegisterType>
    {
        IGenericRepository<RegisterType> _regularPersonalDataTypeRepository;
        IGenericRepository<LocalRegisterType> _localregularPersonalDataTypeRepository;
        IGenericRepository<AttachedOption> _AttachedOptionRepository;

        public AttachedOptionsRegisterTypesController(IGenericRepository<AttachedOption> repository,
                IAuthenticationService authService,
                IGenericRepository<RegisterType> registerTypeRepository,
                IGenericRepository<LocalRegisterType> localRegisterTypeRepository, IGenericRepository<AttachedOption> optionsRepository)
               : base(repository, authService, registerTypeRepository, localRegisterTypeRepository){

            _regularPersonalDataTypeRepository = registerTypeRepository;
            _localregularPersonalDataTypeRepository = localRegisterTypeRepository;
            _AttachedOptionRepository = optionsRepository;
        }

            [System.Web.Http.HttpGet]
            [EnableQuery]
            [ODataRoute("GetRegisterTypesByObjectID(id={id})")]
            public IHttpActionResult GetRegisterTypesByObjectID(int id)
            {
            return GetOptionsByObjectIDAndType(id, EntityType.ITSYSTEMUSAGE, OptionType.REGISTERTYPEDATA);
            }
        

        //move to service
        private IHttpActionResult GetOptionsByObjectIDAndType(int id, EntityType entitytype, OptionType optiontype)
        {
            if (UserId == 0)
                return Unauthorized();
            var orgId = _authService.GetCurrentOrganizationId(UserId);

            var globalOptionData = _regularPersonalDataTypeRepository.AsQueryable().Where(s => s.IsEnabled);
            var localpersonalData = _localregularPersonalDataTypeRepository.AsQueryable().Where(p => p.IsActive && p.OrganizationId == orgId).ToList();

            List<RegisterType> result = new List<RegisterType>();
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

        [System.Web.Http.HttpPost]
        [ODataRoute("AttachedOptions")]
        public async Task<IHttpActionResult> Post([FromODataUri] int key, AttachedOption dto)
        {
            //try
            //{
            //    var Entities = _AttachedOptionRepository.Get();

            //    if (Entities.Count() > 0)
            //    {
            //        entity.Priority = _AttachedOptionRepository.Get().Max(e => e.Priority) + 1;
            //    }
            //    else
            //    {
            //        entity.Priority = 1;
            //    }
            //}
            //catch (Exception e)
            //{
            //    var message = e.Message;
            //    return InternalServerError(e);
            //}

            return base.Post(dto);
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