using System;
using System.Net.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class RiskController : GenericContextAwareApiController<Risk, RiskDTO>
    {
        public RiskController(IGenericRepository<Risk> repository) : base(repository)
        {
        }

        public HttpResponseMessage GetByProject(bool? getByProject, int projectId)
        {
            try
            {
                var risks = Repository.Get(r => r.ItProjectId == projectId);

                return Ok(Map(risks));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
