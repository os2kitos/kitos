using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ArchivePeriodController : GenericApiController<ArchivePeriod, ArchivePeriodDTO>
    {
        // GET: ArchivePeriod
        /// <summary>
        /// Arkiveringsperiode for it system anvedelse på fanen arkivering
        /// </summary>
        /// <param name="repository"></param>
        public ArchivePeriodController(IGenericRepository<ArchivePeriod> repository)
                    : base(repository)
        {
        }
        /// <summary>
        /// Henter en enkelt enhed fra it system anvendelsens arkiv periode 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="system"></param>
        /// <returns></returns>
        public HttpResponseMessage GetSingle(int id, [FromUri] bool system)
        {
            var item = Repository.Get(x => x.ItSystemUsageId == id);

            if (item == null)
                return NotFound();

            return Ok(Map(item));
        }
    }
}