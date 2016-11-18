using Core.DomainModel;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData
{
    public class BaseAdviceController<T> : BaseEntityController<ExternalReference>
    {
        public BaseAdviceController(): base(null,null){ 
        
        }

        [EnableQuery]
        [ODataRoute("/CreateAdvice/")]
        public IHttpActionResult Create(int orgId, int contractId)
        {
            // TODO create
           
            return null;
        }

        [EnableQuery]
        [ODataRoute("/ReadAdvice/({ObjectId})")]
        public IHttpActionResult Read(int ObjectId)
        {
            // TODO create

            return null;
        }

        [EnableQuery]
        [ODataRoute("/UpdateAdvice/")]
        public IHttpActionResult Update(int orgId, int contractId)
        {
            // TODO create

            return null;
        }

        [EnableQuery]
        [ODataRoute("/DeleteAdvice/({ObjectId})")]
        public IHttpActionResult Delete(int ObjectId)
        {
            // TODO create

            return null;
        }



    }
}