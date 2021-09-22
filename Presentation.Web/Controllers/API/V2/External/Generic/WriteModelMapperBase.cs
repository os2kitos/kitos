using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
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

        /// <param name="enforceFallbackIfNotProvided">If set to true, the fallback strategy will be applied even if the data property was not provided in the request</param>
        protected TSection WithResetDataIfPropertyIsDefined<TSection>(TSection deserializedValue, string expectedSectionKey, bool enforceFallbackIfNotProvided = false) where TSection : new()
        {
            var response = deserializedValue;
            if (ClientRequestsChangeTo(expectedSectionKey) || enforceFallbackIfNotProvided)
            {
                response = deserializedValue ?? new TSection();
            }

            return response;
        }
        
        /// <param name="enforceFallbackIfNotProvided">If set to true, the fallback strategy will be applied even if the data property was not provided in the request</param>
        protected TSection WithResetDataIfPropertyIsDefined<TSection>(TSection deserializedValue, string expectedSectionKey, Func<TSection> fallbackFactory, bool enforceFallbackIfNotProvided = false)
        {
            var response = deserializedValue;
            if (ClientRequestsChangeTo(expectedSectionKey) || enforceFallbackIfNotProvided)
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

        protected static ChangedValue<Maybe<IEnumerable<UserRolePair>>> BaseMapRoleAssignments(IReadOnlyCollection<RoleAssignmentRequestDTO> roleAssignmentResponseDtos)
        {
            return (roleAssignmentResponseDtos.Any() ?
                Maybe<IEnumerable<UserRolePair>>.Some(roleAssignmentResponseDtos.Select(x => new UserRolePair
                {
                    RoleUuid = x.RoleUuid,
                    UserUuid = x.UserUuid
                })) :
                Maybe<IEnumerable<UserRolePair>>.None).AsChangedValue();
        }
    }
}