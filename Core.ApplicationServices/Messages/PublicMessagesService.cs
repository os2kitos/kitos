using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Messages;
using Core.DomainModel;
using Core.DomainServices;
using Serilog;

namespace Core.ApplicationServices.Messages
{
    public class PublicMessagesService : IPublicMessagesService
    {
        private readonly IGenericRepository<Text> _repository;
        private readonly IOrganizationalUserContext _organizationalUserContext;
        private readonly ILogger _logger;

        public PublicMessagesService(
            IGenericRepository<Text> repository,
            IOrganizationalUserContext organizationalUserContext,
            ILogger logger)
        {
            _repository = repository;
            _organizationalUserContext = organizationalUserContext;
            _logger = logger;
        }

        public ResourcePermissionsResult GetPermissions()
        {
            var allowModify = _organizationalUserContext.IsGlobalAdmin();
            return new ResourcePermissionsResult(true, allowModify, false);
        }

        public PublicMessages GetPublicMessages()
        {
            var texts = _repository.AsQueryable().ToDictionary(x => x.Id, x => x.Value ?? "");
            return new PublicMessages(
                MapText(texts, Text.SectionIds.About),
                MapText(texts, Text.SectionIds.Guides),
                MapText(texts, Text.SectionIds.StatusMessages),
                MapText(texts, Text.SectionIds.Misc),
                MapText(texts, Text.SectionIds.ContactInfo)
            );
        }

        public Result<PublicMessages, OperationError> UpdateMessages(WritePublicMessagesParams parameters)
        {
            throw new NotImplementedException();
        }

        private string MapText(IReadOnlyDictionary<int, string> textMap, int textId)
        {
            if (textMap.TryGetValue(textId, out var text))
            {
                return text;
            }

            _logger.Error("Missing text id for the front page {textId}. Returning empty text", textId);
            return string.Empty;
        }
    }
}
