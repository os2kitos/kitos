using Core.ApplicationServices.Notification;
using Core.DomainModel.Notification;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Presentation.Web.Controllers.OData
{
    /// <summary>
    /// Search API used for UserNotifications
    /// </summary>
    [InternalApi]
    [ODataRoutePrefix("UserNotifications")]
    public class UserNotificationsController : BaseOdataController
    {
        private readonly IUserNotificationService _notificationService;

        public UserNotificationsController(IUserNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [EnableQuery]
        [RequireTopOnOdataThroughKitosToken]
        [ODataRoute]
        public IHttpActionResult Get([FromODataUri] int organizationId, [FromODataUri] int userId)
        {
            return
                _notificationService
                    .GetNotificationsForUser(organizationId, userId)
                    .Match(onSuccess: Ok, onFailure: FromOperationError);
        }
    }
}