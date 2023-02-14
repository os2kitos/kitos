using Core.Abstractions.Types;
using Core.DomainModel.Advice;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public interface INotificationWriteModelMapper
    {
        Result<Advice, OperationError> FromImmediatePOST(ImmediateNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType);
        Result<Advice, OperationError> FromScheduledPOST(ScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType);
        Result<Advice, OperationError> FromScheduledPUT(UpdateScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType);
    }
}
