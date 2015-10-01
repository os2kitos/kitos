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

        /// <summary>
        /// Gets a specific role for a given user withing a given organization with that specific rolename
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="userId"></param>
        /// <param name="organizationId"></param>
        /// <param name="orgRightsForUserWithRole"></param>
        /// <returns></returns>
        public virtual HttpResponseMessage GetExistsOrgRightsForUserWithRole(string roleName, int userId, int organizationId, bool? orgRightsForUserWithRole)
        {
            try
            {
                var theRights = RightRepository.Get(x => x.Role.Name == roleName && x.UserId == userId && x.ObjectId == organizationId);
                return Ok(theRights.Any());
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public virtual HttpResponseMessage GetRightsWithRoleName(string roleName, bool? roleWithName)
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

        /// <summary>
        /// Deletes all adminRights for a given user withing a given organization
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <param name="byOrganization"></param>
        /// <returns></returns>
        public HttpResponseMessage DeleteByOrganization(int orgId, int userId, bool? byOrganization)
        {
            try
            {
                var rights = RightRepository.Get(r => r.ObjectId == orgId && r.UserId == userId).Select(x => x.Id);

                foreach (var right in rights)
                {
                    RightRepository.DeleteByKey(right);
                }

                RightRepository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostRightByOrganizationRight(bool? rightByOrganizationRight, int organizationId, int userId, AdminRight right)
        {
            try
            {
                // if user has any role within the organization (or global admin) they should be able to add new adminrights
                if (!KitosUser.IsGlobalAdmin)
                    if (!RightRepository.Get(r => r.UserId == userId && r.ObjectId == organizationId).Any())
                        return Unauthorized();

                right.ObjectId = organizationId;
                right.ObjectOwner = KitosUser;
                right.LastChangedByUser = KitosUser;
                right.LastChanged = DateTime.UtcNow;

                RightRepository.Insert(right);
                RightRepository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
