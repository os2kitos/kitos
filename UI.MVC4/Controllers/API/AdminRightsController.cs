using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class AdminRightsController : GenericRightsController<Organization, AdminRight, AdminRole>
    {
        public AdminRightsController(IGenericRepository<AdminRight> rightRepository, IGenericRepository<Organization> objectRepository) : base(rightRepository, objectRepository)
        {
        }

        public virtual HttpResponseMessage GetAllRights(bool? rights)
        {
            try
            {
                if (!IsGlobalAdmin()) return Unauthorized();
                var theRights = RightRepository.Get();
                var dtos = Map<IEnumerable<AdminRight>, IEnumerable<RightOutputDTO>>(theRights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
