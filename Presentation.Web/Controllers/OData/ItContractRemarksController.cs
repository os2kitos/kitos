using System.Net;
using System.Web.Http;
using System.Web.OData.Routing;
using Core.DomainModel.ItContract;
using Core.ApplicationServices;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItContractRemarksController : BaseEntityController<ItContractRemark>
    {
        private readonly IAuthenticationService _authService;
        private readonly IGenericRepository<ItContractRemark> _repository;

        public ItContractRemarksController(IGenericRepository<ItContractRemark> repository, IAuthenticationService authService) : base(repository, authService)
        {
            _repository = repository;
            _authService = authService;
        }

        [ODataRoute("ItContracts({int contractKey})/Remark")]
        public IHttpActionResult GetRemark(int contractKey)
        {
            var remark = _repository.GetByKey(contractKey);

            if (remark == null)
            {
                return NotFound();
            }

            if (_authService.HasWriteAccess(UserId, remark) || _authService.IsGlobalAdmin(UserId) || _authService.IsLocalAdmin(UserId) || remark.ObjectOwnerId == UserId)
            {
                return Ok(remark);
            }

            return StatusCode(HttpStatusCode.Forbidden);

        }
    }
}