using System.Linq;
using System.Web.OData;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class OrganizationsController : BaseController<Organization>
    {
        public OrganizationsController(IGenericRepository<Organization> repository)
            : base(repository)
        {
        }

        //// GET /Organizations(1)/ItSystems
        //[EnableQuery]
        //public IQueryable<ItSystem> GetItSystems([FromODataUriAttribute] int key)
        //{
        //    var result = Repository.AsQueryable().Where(m => m.Id == key).SelectMany(x => x.ItSystems);
        //    return result;
        //}
    }
}
