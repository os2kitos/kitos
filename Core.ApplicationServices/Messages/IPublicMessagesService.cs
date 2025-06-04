using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Messages;
using Core.DomainModel.PublicMessage;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Messages
{
    public interface IPublicMessagesService
    {
        ResourcePermissionsResult GetPermissions();
        IEnumerable<PublicMessage> Read();
        Result<PublicMessage, OperationError> Patch(Guid messageUuid, WritePublicMessagesParams parameters);
        Result<PublicMessage, OperationError> Create(WritePublicMessagesParams parameters);
    }
}
