using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [ControllerEvaluationCompleted]
    public class ItSystemUsageRightsController : GenericRightsController<ItSystemUsage, ItSystemRight, ItSystemRole>
    {
        public ItSystemUsageRightsController(
            IGenericRepository<ItSystemRight> rightRepository, 
            IGenericRepository<ItSystemUsage> objectRepository,
            IAuthorizationContext authorizationContext) 
            : base(rightRepository, objectRepository, authorizationContext)
        { }

        /// <summary>
        /// Returns all ITSystemRights for a specific user
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns>List of rights</returns>
        [DeprecatedApi]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<ICollection<RightOutputDTO>>))]
        public HttpResponseMessage GetRightsForUser(int userId)
        {
            try
            {
                var theRights = new List<ItSystemRight>();
                theRights.AddRange(RightRepository.Get(r => r.UserId == userId));

                var dtos = AutoMapper.Mapper.Map<ICollection<ItSystemRight>, ICollection<RightOutputDTO>>(theRights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
