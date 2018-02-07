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

    public class AttachedOptionsRegularPersonalDataController : AttachedOptionsController
    {
        IGenericRepository<AttachedOption> _optionRepository;
        IGenericRepository<ItSystem> _itSystemRepository;
        IGenericRepository<RegularPersonalDataType> _regularPersonalDataTypeRepository;
        IGenericRepository<LocalRegularPersonalDataType> _localregularPersonalDataTypeRepository;


        public AttachedOptionsRegularPersonalDataController(IGenericRepository<AttachedOption> repository, IAuthenticationService authService, IGenericRepository<ItSystem> itSystemRepository,
            IGenericRepository<RegularPersonalDataType> regularPersonalDataTypeRepository,
            IGenericRepository<LocalRegularPersonalDataType> localregularPersonalDataTypeRepository)
           : base(repository, authService, itSystemRepository)
        {
            _regularPersonalDataTypeRepository = regularPersonalDataTypeRepository;
            _localregularPersonalDataTypeRepository = localregularPersonalDataTypeRepository;
        }

        [System.Web.Http.HttpGet]
        [EnableQuery]
        [ODataRoute("GetRegularPersonalDataByObjectID(id={id})")]
        public IHttpActionResult GetRegularPersonalDataByObjectIDAndType(int id)
        {
            if (UserId == 0)
                return Unauthorized();

            var globalOptionData = _regularPersonalDataTypeRepository.AsQueryable().Where(s => s.IsEnabled || s.IsObligatory);
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

            var options = GetAttachedOptions(OptionType.REGULARPERSONALDATA, id);

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