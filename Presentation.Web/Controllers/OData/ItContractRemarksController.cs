using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData.Routing;
using Core.DomainModel.ItContract;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItContractRemarksController : BaseEntityController<ItContractRemark>
    {
        private readonly IAuthenticationService _authService;
        private readonly IGenericRepository<ItContract> _contractRepository;

        public ItContractRemarksController(IGenericRepository<ItContractRemark> remarkRepository, IGenericRepository<ItContract> contractRepository, IAuthenticationService authService) : base(remarkRepository, authService)
        {
            _authService = authService;
            _contractRepository = contractRepository;
        }

        //GET ItContracts({contractKey})/Remark
        [ODataRoute("ItContracts({contractKey})/Remark")]
        public IHttpActionResult GetRemark(int contractKey)
        {
            var remark = Repository.AsQueryable().FirstOrDefault(r => r.Id == contractKey);

            if (remark == null)
            {
                return NotFound();
            }

            if (_authService.HasReadAccess(UserId, remark))
            {
                return Ok(remark);
            }

            return StatusCode(HttpStatusCode.Unauthorized);
        }

        //POST /ItContractRemarks
        [ODataRoute("ItContractRemarks({contractKey})")]
        public IHttpActionResult PostRemark(int contractKey, ItContractRemark remark)
        {
            var contract = _contractRepository.AsQueryable().FirstOrDefault(c => c.Id == contractKey);

            if (contract == null)
            {
                return NotFound();
            }

            remark.ItContract = contract;

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (remark is IHasOrganization && (remark as IHasOrganization).OrganizationId == 0)
            {
                (remark as IHasOrganization).OrganizationId = _authService.GetCurrentOrganizationId(UserId);
            }

            if (!_authService.HasWriteAccess(UserId, remark))
            {
                return Unauthorized();
            }

            try
            {
                remark.ObjectOwnerId = UserId;
                remark.LastChangedByUserId = UserId;
                var entityWithOrganization = remark as IHasOrganization;
                if (entityWithOrganization != null)
                {
                    entityWithOrganization.OrganizationId = _authService.GetCurrentOrganizationId(UserId);
                }
                remark = Repository.Insert(remark);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return Created(remark);
        }
    }
}