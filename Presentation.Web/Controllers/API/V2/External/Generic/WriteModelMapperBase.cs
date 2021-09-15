using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Shared.Write;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public abstract class WriteModelMapperBase
    {
        private readonly Lazy<ISet<string>> _currentRequestRootProperties;

        protected WriteModelMapperBase(ICurrentHttpRequest currentHttpRequest)
        {
            _currentRequestRootProperties = new Lazy<ISet<string>>(currentHttpRequest.GetDefinedJsonRootProperties);
        }

        protected TSection WithResetDataIfPropertyIsDefined<TSection>(TSection deserializedValue, string expectedSectionKey) where TSection : new()
        {
            var response = deserializedValue;
            if (ClientRequestsChangeTo(expectedSectionKey))
            {
                response = deserializedValue ?? new TSection();
            }

            return response;
        }

        protected TSection WithResetDataIfPropertyIsDefined<TSection>(TSection deserializedValue, string expectedSectionKey, Func<TSection> fallbackFactory)
        {
            var response = deserializedValue;
            if (ClientRequestsChangeTo(expectedSectionKey))
            {
                response = deserializedValue ?? fallbackFactory();
            }

            return response;
        }

        protected bool ClientRequestsChangeTo(string expectedSectionKey)
        {
            return _currentRequestRootProperties.Value.Contains(expectedSectionKey);
        }

        protected IEnumerable<UpdatedExternalReferenceProperties> BaseMapReferences(IEnumerable<ExternalReferenceDataDTO> references)
        {
            return references.Select(x => new UpdatedExternalReferenceProperties
            {
                Title = x.Title,
                DocumentId = x.DocumentId,
                Url = x.Url,
                MasterReference = x.MasterReference
            });
        }
    }
}