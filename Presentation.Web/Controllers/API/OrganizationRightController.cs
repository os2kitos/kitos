using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainServices;
using Presentation.Web.Models;
using System.Web.Http;
using Core.DomainModel.Organization;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class OrganizationRightController : GenericApiController<OrganizationRight, OrganizationRightDTO>
    {
        private readonly IGenericRepository<OrganizationRight> _rightRepository;

        public OrganizationRightController(IGenericRepository<OrganizationRight> rightRepository) : base (rightRepository)
        {
            _rightRepository = rightRepository;
        }

        public virtual HttpResponseMessage GetRightsWithRoleName(string roleName, bool? roleWithName)
        {
            try
            {
                if (!IsGlobalAdmin())
                {
                    return Forbidden();
                }
                var role = (OrganizationRole)Enum.Parse(typeof(OrganizationRole), roleName, true);
                var theRights = _rightRepository.Get(x => x.Role == role);
                var dtos = Map<IEnumerable<OrganizationRight>, IEnumerable<OrganizationRightDTO>>(theRights);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Returns all rights for an object
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <returns>List of all rights</returns>
        protected IEnumerable<OrganizationRight> GetRightsQuery(int id)
        {
            return _rightRepository.Get(right => right.OrganizationId == id);
        }

        /// <summary>
        /// Delete a right from the object
        /// </summary>
        /// <param name="id">ID of object</param>
        /// <param name="rId">ID of role</param>
        /// <param name="uId">ID of user in role</param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public HttpResponseMessage Delete(int id, [FromUri] int rId, [FromUri] int uId, int organizationId)
        {
            try
            {
                var right = _rightRepository.Get(r => r.OrganizationId == id && r.Role == (OrganizationRole)rId && r.UserId == uId).FirstOrDefault();

                if (right == null)
                {
                    return NotFound();
                }

                // Only global admin can set other users as global admins
                if (right.Role == OrganizationRole.GlobalAdmin)
                {
                    if (!KitosUser.IsGlobalAdmin)
                        return Forbidden();
                }

                // Only local and global admins can make users local admins
                if (right.Role == OrganizationRole.LocalAdmin)
                {
                    if (!KitosUser.IsGlobalAdmin && !KitosUser.IsLocalAdmin)
                        return Forbidden();
                }

                if(!base.HasWriteAccess(right, KitosUser, organizationId))
                {
                    return Forbidden();
                }


                _rightRepository.DeleteByKey(right.Id);
                _rightRepository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
