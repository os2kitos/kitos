using System;
using System.Collections.Generic;
using System.Net.Http;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItSystemUsageRightsController : GenericRightsController<ItSystemUsage, ItSystemRight, ItSystemRole>
    {
        public ItSystemUsageRightsController(IGenericRepository<ItSystemRight> rightRepository, IGenericRepository<ItSystemUsage> objectRepository) : base(rightRepository, objectRepository)
        {}

        /// <summary>
        /// Returns all ITSystemRights for a specific user
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns>List of rights</returns>
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
