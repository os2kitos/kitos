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

    public class AttachedOptionsSensitivePersonalDataControllerController : AttachedOptionsController
    {
        IGenericRepository<AttachedOption> _optionRepository;
        IGenericRepository<ItSystem> _itSystemRepository;
        IGenericRepository<SensitivePersonalDataType> _sensitiveDataTypeRepository;
        IGenericRepository<LocalSensitivePersonalDataType> _localSensitivePersonalDataTypeRepository;


        public AttachedOptionsSensitivePersonalDataControllerController(IGenericRepository<AttachedOption> repository, IAuthenticationService authService, IGenericRepository<ItSystem> itSystemRepository,
            IGenericRepository<LocalSensitivePersonalDataType> localSensitivePersonalDataTypeRepository,
            IGenericRepository<SensitivePersonalDataType> sensitiveDataTypeRepository)
           : base(repository, authService, itSystemRepository)
        {
            _optionRepository = repository;
            _itSystemRepository = itSystemRepository;
            _localSensitivePersonalDataTypeRepository = localSensitivePersonalDataTypeRepository;
            _sensitiveDataTypeRepository = sensitiveDataTypeRepository;
        }

        [System.Web.Http.HttpGet]
        [EnableQuery]
        [ODataRoute("GetSensitivePersonalDataByObjectID(id={id})")]
        public IHttpActionResult GetSensitivePersonalDataByObjectIDAndType(int id)
        {
            if (UserId == 0)
                return Unauthorized();

            var globalOptionData = _sensitiveDataTypeRepository.AsQueryable().Where(s => s.IsEnabled || (s.IsEnabled && s.IsObligatory));
            var localpersonalData = _localSensitivePersonalDataTypeRepository.AsQueryable().Where(p => p.IsActive).ToList();

            List<SensitivePersonalDataType> result = new List<SensitivePersonalDataType>();
            result.AddRange(globalOptionData.AsQueryable().Where(s => s.IsObligatory));

            foreach (var p in localpersonalData)
            {
                var data = globalOptionData.AsQueryable().FirstOrDefault(s => s.Id == p.OptionId && (s.IsEnabled && !s.IsObligatory));
                if (data != null)
                {
                    result.Add(data);
                }
            }

            var options = GetAttachedOptions(OptionType.SENSITIVEPERSONALDATA, id);

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
    }
}