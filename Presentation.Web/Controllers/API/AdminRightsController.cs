using System;
using System.Collections.Generic;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class AdminRightsController : GenericRightsController<Organization, AdminRight, AdminRole>
    {
        public AdminRightsController(IGenericRepository<AdminRight> rightRepository, IGenericRepository<Organization> objectRepository) 
            : base(rightRepository, objectRepository)
        {
        }

        public virtual HttpResponseMessage GetAllRights()
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
