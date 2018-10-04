using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.OData;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    using System.Net;
    using System.Web.Http.Description;

    [ApiExplorerSettings(IgnoreApi = true)]
    public class AttachedOptionsRegularPersonalDataController : AttachedOptionsFunctionController<ItSystem, RegularPersonalDataType, LocalRegularPersonalDataType>
    {
        IGenericRepository<RegularPersonalDataType> _regularPersonalDataTypeRepository;
        IGenericRepository<LocalRegularPersonalDataType> _localregularPersonalDataTypeRepository;
        IGenericRepository<AttachedOption> _AttachedOptionRepository;

        public AttachedOptionsRegularPersonalDataController(IGenericRepository<AttachedOption> repository, IAuthenticationService authService,
            IGenericRepository<RegularPersonalDataType> regularPersonalDataTypeRepository,
            IGenericRepository<LocalRegularPersonalDataType> localregularPersonalDataTypeRepository, IGenericRepository<AttachedOption> optionsRepository)
           : base(repository, authService, regularPersonalDataTypeRepository,
                 localregularPersonalDataTypeRepository){

            _regularPersonalDataTypeRepository = regularPersonalDataTypeRepository;
            _localregularPersonalDataTypeRepository = localregularPersonalDataTypeRepository;
            _AttachedOptionRepository = optionsRepository;
        }

        [System.Web.Http.HttpGet]
        [EnableQuery]
        [ODataRoute("GetRegularPersonalDataBySystemId(id={id})")]
        public IHttpActionResult GetRegularPersonalDataBySystemId(int id)
        {
            return GetOptionsByObjectIDAndType(id, EntityType.ITSYSTEM, OptionType.REGULARPERSONALDATA);
        }

        [System.Web.Http.HttpGet]
        [EnableQuery]
        [ODataRoute("GetRegularPersonalDataByUsageId(id={id})")]
        public IHttpActionResult GetRegularPersonalDataByUsageId(int id)
        {
            return GetOptionsByObjectIDAndType(id, EntityType.ITSYSTEMUSAGE, OptionType.REGULARPERSONALDATA);
        }




        private IHttpActionResult GetOptionsByObjectIDAndType(int id, EntityType entitytype, OptionType optiontype)
        {
            if (UserId == 0)
                return Unauthorized();

            var globalOptionData = _regularPersonalDataTypeRepository.AsQueryable().Where(s => s.IsEnabled || (s.IsEnabled && s.IsObligatory));
            var localpersonalData = _localregularPersonalDataTypeRepository.AsQueryable().Where(p => p.IsActive).ToList();

            List<RegularPersonalDataType> result = new List<RegularPersonalDataType>();
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