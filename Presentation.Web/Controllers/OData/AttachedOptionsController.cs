using Core.ApplicationServices;
using Core.DomainServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using System.Web.Http;
using Ninject.Infrastructure.Language;
using System.Linq;
using System.Web.OData;
using System.Web.OData.Routing;
using System.Collections.Generic;
using System.Net;
using System;

namespace Presentation.Web.Controllers.OData
{
    public class AttachedOptionsController : BaseEntityController<AttachedOption>
    {
        IAuthenticationService _authService;
        IGenericRepository<AttachedOption> _optionRepository;
        IGenericRepository<ItSystem> _itSystemRepository;
        IGenericRepository<RegularPersonalDataType> _regularPersonalDataTypeRepository;
        IGenericRepository<LocalRegularPersonalDataType> _localregularPersonalDataTypeRepository;
        IGenericRepository<SensitivePersonalDataType> _sensitiveDataTypeRepository;
        IGenericRepository<LocalSensitivePersonalDataType> _localSensitivePersonalDataTypeRepository;


        public AttachedOptionsController(IGenericRepository<AttachedOption> repository, IAuthenticationService authService, IGenericRepository<ItSystem> itSystemRepository,
            IGenericRepository<RegularPersonalDataType> regularPersonalDataTypeRepository,
            IGenericRepository<LocalRegularPersonalDataType> localregularPersonalDataTypeRepository,
            IGenericRepository<LocalSensitivePersonalDataType> localSensitivePersonalDataTypeRepository,
            IGenericRepository<SensitivePersonalDataType> sensitiveDataTypeRepository)
           : base(repository, authService)
        {
            _authService = authService;
            _optionRepository = repository;
            _itSystemRepository = itSystemRepository;
            _regularPersonalDataTypeRepository = regularPersonalDataTypeRepository;
            _localregularPersonalDataTypeRepository = localregularPersonalDataTypeRepository;
            _localSensitivePersonalDataTypeRepository = localSensitivePersonalDataTypeRepository;
            _sensitiveDataTypeRepository = sensitiveDataTypeRepository;
        }
        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetRegularPersonalDataByObjectID(id={id})")]
        public IHttpActionResult GetRegularPersonalDataByObjectIDAndType(int id)
        {
            if (UserId == 0)
                return Unauthorized();

            var localpersonalData = _localregularPersonalDataTypeRepository.AsQueryable().Where(p => p.IsActive).ToList();

            List<RegularPersonalDataType> result = new List<RegularPersonalDataType>();

            foreach (var p in localpersonalData){
                var data = _regularPersonalDataTypeRepository.AsQueryable().FirstOrDefault(s => s.Id == p.OptionId && s.IsEnabled);
                    if(data != null) { 
                        result.Add(data);
                }
            }
            var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(AttachedOption));
            var system = _itSystemRepository.AsQueryable().FirstOrDefault(s => s.Id == id);

            if (system != null)
            {

                List<AttachedOption> options;

                if (_authService.HasReadAccessOutsideContext(UserId) || hasOrg == false)
                {
                    //tolist so we can operate with open datareaders in the following foreach loop.
                    options = _optionRepository.AsQueryable().Where(x => x.ObjectId == system.Id && x.OptionType == OptionType.REGULARPERSONALDATA).ToList();
                }
                else
                {
                    options = _optionRepository.AsQueryable()
                     .Where(x => ((IHasOrganization)x).OrganizationId == _authService.GetCurrentOrganizationId(UserId) && x.ObjectId == id && x.OptionType == OptionType.REGULARPERSONALDATA).ToList();
                }
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
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetSensitivePersonalDataByObjectID(id={id})")]
        public IHttpActionResult GetSensitivePersonalDataByObjectIDAndType(int id)
        {
            if (UserId == 0)
                return Unauthorized();

            var localpersonalData = _localSensitivePersonalDataTypeRepository.AsQueryable().Where(p => p.IsActive).ToList();

            List<SensitivePersonalDataType> result = new List<SensitivePersonalDataType>();

            foreach (var p in localpersonalData)
            {
                var data = _sensitiveDataTypeRepository.AsQueryable().FirstOrDefault(s => s.Id == p.OptionId && s.IsEnabled);
                if (data != null)
                {
                    result.Add(data);
                }
            }
            var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(AttachedOption));
            var system = _itSystemRepository.AsQueryable().FirstOrDefault(s => s.Id == id);

            if (system != null)
            {

                List<AttachedOption> options;

                if (_authService.HasReadAccessOutsideContext(UserId) || hasOrg == false)
                {
                    //tolist so we can operate with open datareaders in the following foreach loop.
                    options = _optionRepository.AsQueryable().Where(x => x.ObjectId == system.Id && x.OptionType == OptionType.SENSITIVEPERSONALDATA).ToList();
                }
                else
                {
                    options = _optionRepository.AsQueryable()
                     .Where(x => ((IHasOrganization)x).OrganizationId == _authService.GetCurrentOrganizationId(UserId) && x.ObjectId == id && x.OptionType == OptionType.REGULARPERSONALDATA).ToList();
                }
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
                return NotFound();
            }

            return Ok(result);
        }

        [HttpDelete]
        [EnableQuery]
        [ODataRoute("RemoveOption(id={id}, systemId={systemId}, type={type})")]
        public IHttpActionResult RemoveOption(int id, int systemId, OptionType type)
        {
            var option = _optionRepository.AsQueryable().FirstOrDefault(o => o.OptionId == id && o.ObjectId == systemId && o.OptionType == type);

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