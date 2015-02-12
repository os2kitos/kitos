using System;
using System.Collections.Generic;
using System.Linq;
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

        public virtual HttpResponseMessage GetAllRightsWithRoleName(string roleName, bool roleWithName)
        {
            try
            {
                if (!IsGlobalAdmin()) return Unauthorized();
                var theRights = RightRepository.Get(x => x.Role.Name == roleName);
                var dtos = Map<IEnumerable<AdminRight>, IEnumerable<RightOutputDTO>>(theRights);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Removes AdminRights for a given user within a given organization
        /// </summary>
        /// <param name="orgId">OrganizationId</param>
        /// <param name="uId">UserId</param>
        /// <returns></returns>
        public HttpResponseMessage DeleteByOrganizationRole(int orgId, int uId)
        {
            try
            {
                var rId = RightRepository.Get().Where(r => r.ObjectId == orgId && r.UserId == uId);
                
                if(rId.Any()) RightRepository.DeleteByKey(rId);

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
