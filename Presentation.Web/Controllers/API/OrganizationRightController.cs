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
    [ControllerEvaluationCompleted]
    public class OrganizationRightController : GenericApiController<OrganizationRight, OrganizationRightDTO>
    {
        private readonly IGenericRepository<OrganizationRight> _rightRepository;
        private readonly IGenericRepository<Organization> _objectRepository;

        public OrganizationRightController(IGenericRepository<OrganizationRight> rightRepository, IGenericRepository<Organization> objectRepository) : base (rightRepository)
        {
            _rightRepository = rightRepository;
            _objectRepository = objectRepository;
        }

        [DeprecatedApi]
        public virtual HttpResponseMessage GetAllRights()
        {
            try
            {
                if (!IsGlobalAdmin())
                {
                    return Forbidden();
                }
                var theRights = _rightRepository.Get();
                var dtos = Map<IEnumerable<OrganizationRight>, IEnumerable<OrganizationRightDTO>>(theRights);

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
        [DeprecatedApi]
        public virtual HttpResponseMessage GetExistsOrgRightsForUserWithRole(string roleName, int userId, int organizationId, bool? orgRightsForUserWithRole)
        {
            try
            {
                var role = (OrganizationRole)Enum.Parse(typeof(OrganizationRole), roleName, true);
                var theRights = _rightRepository.Get(x => x.Role == role && x.UserId == userId && x.OrganizationId == organizationId);
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
        /// Removes OrganizationRights for a given user within a given organization
        /// </summary>
        /// <param name="orgId">OrganizationId</param>
        /// <param name="uId">UserId</param>
        /// <returns></returns>
        [DeprecatedApi]
        public HttpResponseMessage DeleteByOrganizationRole(int orgId, int uId)
        {
            try
            {
                var rId = _rightRepository.Get().Where(r => r.OrganizationId == orgId && r.UserId == uId);

                if (rId.Any()) _rightRepository.DeleteByKey(rId);

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
        [DeprecatedApi]
        public HttpResponseMessage DeleteByOrganization(int orgId, int userId, bool? byOrganization)
        {
            try
            {
                var rights = _rightRepository.Get(r => r.OrganizationId == orgId && r.UserId == userId).Select(x => x.Id);

                foreach (var right in rights)
                {
                    _rightRepository.DeleteByKey(right);
                }

                _rightRepository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [DeprecatedApi]
        public HttpResponseMessage PostRightByOrganizationRight(bool? rightByOrganizationRight, int organizationId, OrganizationRightDTO dto)
        {
            try
            {
                var right = AutoMapper.Mapper.Map<OrganizationRightDTO, OrganizationRight>(dto);
                
                // Only global admin can set other users as global admins
                if(right.Role == OrganizationRole.GlobalAdmin)
                {
                    if (!KitosUser.IsGlobalAdmin)
                        return Forbidden();
                }

                // Only local and global admins can make users local admins
                if(right.Role == OrganizationRole.LocalAdmin)
                {
                    if(!KitosUser.IsGlobalAdmin && !KitosUser.IsLocalAdmin)
                        return Forbidden();
                }

                right.OrganizationId = organizationId;
                right.ObjectOwner = KitosUser;

                if (!base.HasWriteAccess(right, KitosUser, organizationId))
                {
                    return Forbidden();
                }
              
                right.LastChangedByUser = KitosUser;
                right.LastChanged = DateTime.UtcNow;

                _rightRepository.Insert(right);
                _rightRepository.Save();

                return Ok();
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
        /// Get all rights for a given object
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <returns>List of rights</returns>
        [DeprecatedApi]
        public virtual HttpResponseMessage GetRights(int id)
        {
            try
            {
                var theRights = GetRightsQuery(id);
                var dtos = Map<IEnumerable<OrganizationRight>, IEnumerable<OrganizationRightDTO>>(theRights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
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
