using System;
using System.Collections.Generic;
using System.Net.Http;
using Core.DomainServices;
using Presentation.Web.Models;
using System.Web.Http;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class OrganizationRightController : GenericApiController<OrganizationRight, OrganizationRightDTO>
    {
        private readonly IGenericRepository<OrganizationRight> _rightRepository;
        private readonly IOrganizationRightsService _organizationRightsService;

        public OrganizationRightController(
            IGenericRepository<OrganizationRight> rightRepository,
            IOrganizationRightsService organizationRightsService)
            : base(rightRepository)
        {
            _rightRepository = rightRepository;
            _organizationRightsService = organizationRightsService;
        }

        public virtual HttpResponseMessage GetRightsWithRoleName(string roleName, bool? roleWithName)
        {
            try
            {
                if (GetCrossOrganizationReadAccessLevel() < CrossOrganizationDataReadAccessLevel.All)
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
                var result = _organizationRightsService.RemoveRole(organizationId, uId, (OrganizationRole)rId);

                if (result.Ok)
                {
                    return Ok();
                }

                switch (result.Error)
                {
                    case OperationFailure.BadInput:
                        return BadRequest();
                    case OperationFailure.NotFound:
                        return NotFound();
                    case OperationFailure.Forbidden:
                        return Forbidden();
                    case OperationFailure.Conflict:
                        return Conflict(string.Empty);
                    default:
                        return Error(result);
                }
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
