using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItProjectRightController : GenericRightsController<ItProject, ItProjectRight, ItProjectRole>
    {
        public ItProjectRightController(IGenericRepository<ItProjectRight> rightRepository, IGenericRepository<ItProject> objectRepository) : base(rightRepository, objectRepository)
        {
        }

        /// <summary>
        /// Returns all ItProjectRight for a specific user
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns>List of rights</returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<ICollection<RightOutputDTO>>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public HttpResponseMessage GetRightsForUser(int userId)
        {
            try
            {
                var theRights = new List<ItProjectRight>();
                theRights.AddRange(RightRepository.Get(r => r.UserId == userId));

                var dtos = AutoMapper.Mapper.Map<ICollection<ItProjectRight>, ICollection<RightOutputDTO>>(theRights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
