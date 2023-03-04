using System.Web.Http;
using Core.ApplicationServices.Notification;
using Microsoft.AspNet.OData;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class AdviceSentController : BaseOdataController
    {
        private readonly IRegistrationNotificationService _registrationNotificationService;

        public AdviceSentController(IRegistrationNotificationService registrationNotificationService)
        {
            _registrationNotificationService = registrationNotificationService;
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_registrationNotificationService.GetSent());
        }
    }
}