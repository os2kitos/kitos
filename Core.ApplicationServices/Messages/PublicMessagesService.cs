using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Messages;
using Core.ApplicationServices.Model.Shared;
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
            var texts = GetTextsLookup();
            return MapPublicMessages(texts);
        }

        private PublicMessages MapPublicMessages(IReadOnlyDictionary<int, Text> texts)
        {
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
            if (!GetPermissions().Modify)
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            var texts = GetTextsLookup();
            WriteChange(parameters.About, Text.SectionIds.About, texts);
            WriteChange(parameters.ContactInfo, Text.SectionIds.ContactInfo, texts);
            WriteChange(parameters.Guides, Text.SectionIds.Guides, texts);
            WriteChange(parameters.Misc, Text.SectionIds.Misc, texts);
            WriteChange(parameters.StatusMessages, Text.SectionIds.StatusMessages, texts);
            _repository.Save();

            return MapPublicMessages(texts);
        }

        private void WriteChange(OptionalValueChange<string> change, int textId, Dictionary<int, Text> texts)
        {
            if (change.HasChange)
            {
                if (texts.TryGetValue(textId, out var text))
                {
                    text.Value = change.NewValue;
                }
                else
                {
                    _logger.Error("Missing text id for the front page {textId}. Not able to change unknown text", textId);
                }
            }
        }

        private Dictionary<int, Text> GetTextsLookup()
        {
            return _repository.AsQueryable().ToDictionary(x => x.Id, x => x);
        }

        private string MapText(IReadOnlyDictionary<int, Text> textMap, int textId)
        {
            if (textMap.TryGetValue(textId, out var text))
            {
                return text.Value ?? string.Empty;
            }

            _logger.Error("Missing text id for the front page {textId}. Returning empty text", textId);
            return string.Empty;
        }
    }
}
