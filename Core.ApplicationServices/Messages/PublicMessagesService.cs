using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Messages;
using Core.DomainModel.PublicMessage;
using Core.DomainServices;
using Core.DomainServices.Extensions;

namespace Core.ApplicationServices.Messages
{
    public class PublicMessagesService : IPublicMessagesService
    {
        private readonly IGenericRepository<PublicMessage> _repository;
        private readonly IOrganizationalUserContext _organizationalUserContext;

        public PublicMessagesService(
            IGenericRepository<PublicMessage> repository,
            IOrganizationalUserContext organizationalUserContext)
        {
            _repository = repository;
            _organizationalUserContext = organizationalUserContext;
        }

        public ResourcePermissionsResult GetPermissions()
        {
            var allowModify = _organizationalUserContext.IsGlobalAdmin();
            return new ResourcePermissionsResult(true, allowModify, false);
        }

        public IEnumerable<PublicMessage> Read()
        {
            return _repository.AsQueryable().OrderBy(x => x.Id).ToList();
        }
        
        public Result<PublicMessage, OperationError> Create(WritePublicMessagesParams parameters)
        {
            if (!GetPermissions().Modify)
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            var message = new PublicMessage();
            var result = WriteChange(message, parameters);
            if (result.Failed) 
                return result.Error;

            _repository.Insert(message);
            _repository.Save();
            return message;
        }

        public Result<PublicMessage, OperationError> Patch(Guid messageUuid, WritePublicMessagesParams parameters)
        {
            if (!GetPermissions().Modify)
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            var updateResult = GetMessageByUuid(messageUuid)
                .Match(message => WriteChange(message, parameters),
                    () => new OperationError($"Message with uuid: {messageUuid} was not found", OperationFailure.NotFound));

            if (updateResult.Failed)
            {
                return updateResult.Error;
            }
            
            _repository.Save();
            return updateResult.Value;
        }

        private static Result<PublicMessage, OperationError> WriteChange(PublicMessage message, WritePublicMessagesParams parameters)
        {
            return message.WithOptionalUpdate(parameters.LongDescription, (updateText, value) => updateText.UpdateLongDescription(value))
                    .Bind(updatedText => updatedText.WithOptionalUpdate(parameters.Title, (updateText, title) => updateText.UpdateTitle(title)))
                    .Bind(updatedText => updatedText.WithOptionalUpdate(parameters.ShortDescription,
                        (updateText, shortDescription) => updateText.UpdateShortDescription(shortDescription)))
                    .Bind(updatedText =>
                        updatedText.WithOptionalUpdate(parameters.Link,
                            (updateText, link) => updateText.UpdateLink(link)))
                    .Bind(updatedText => updatedText.WithOptionalUpdate(parameters.Status,
                        (updateText, status) => updateText.UpdateStatus(status)));
        }

        private Maybe<PublicMessage> GetMessageByUuid(Guid uuid)
        {
            return _repository.AsQueryable().ByUuid(uuid);
        }
    }
}
