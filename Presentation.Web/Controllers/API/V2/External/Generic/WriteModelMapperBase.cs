using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.Abstractions.Extensions;
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
        private readonly ICurrentHttpRequest _currentHttpRequest;
        private readonly IDictionary<string, HashSet<string>> _currentRequestProperties;

        protected WriteModelMapperBase(ICurrentHttpRequest currentHttpRequest)
        {
            _currentHttpRequest = currentHttpRequest;
            _currentRequestProperties = new Dictionary<string, HashSet<string>>();
        }

        /// <param name="enforceFallbackIfNotProvided">If set to true, the fallback strategy will be applied even if the data property was not provided in the request</param>
        protected TSection WithResetDataIfPropertyIsDefined<TSection>(TSection deserializedValue, string expectedSectionKey, bool enforceFallbackIfNotProvided = false) where TSection : new()
        {
            var response = deserializedValue;
            if (enforceFallbackIfNotProvided || ClientRequestsChangeTo(expectedSectionKey))
            {
                response = deserializedValue ?? new TSection();
            }

            return response;
        }

        /// <param name="enforceFallbackIfNotProvided">If set to true, the fallback strategy will be applied even if the data property was not provided in the request</param>
        protected TSection WithResetDataIfPropertyIsDefined<TSection>(TSection deserializedValue, string expectedSectionKey, Func<TSection> fallbackFactory, bool enforceFallbackIfNotProvided = false)
        {
            var response = deserializedValue;
            if (enforceFallbackIfNotProvided || ClientRequestsChangeTo(expectedSectionKey))
            {
                response = deserializedValue ?? fallbackFactory();
            }

            return response;
        }

        /// <param name="enforceFallbackIfNotProvided">If set to true, the fallback strategy will be applied even if the data property was not provided in the request</param>
        protected TSection WithResetDataIfPropertyIsDefined<TRoot, TSection>(TSection deserializedValue, Expression<Func<TRoot, TSection>> propertySelection, bool enforceFallbackIfNotProvided = false) where TSection : new()
        {
            var response = deserializedValue;
            if (enforceFallbackIfNotProvided || ClientRequestsChangeTo(propertySelection))
            {
                response = deserializedValue ?? new TSection();
            }

            return response;
        }

        /// <param name="enforceFallbackIfNotProvided">If set to true, the fallback strategy will be applied even if the data property was not provided in the request</param>
        protected TSection WithResetDataIfPropertyIsDefined<TRoot, TSection>(TSection deserializedValue, Expression<Func<TRoot, TSection>> propertySelection, Func<TSection> fallbackFactory, bool enforceFallbackIfNotProvided = false)
        {
            var response = deserializedValue;
            if (enforceFallbackIfNotProvided || ClientRequestsChangeTo(propertySelection))
            {
                response = deserializedValue ?? fallbackFactory();
            }

            return response;
        }

        protected bool ClientRequestsChangeTo<TRoot>(Expression<Func<TRoot, object>> propertySelection)
        {
            return ClientRequestsChangeTo<TRoot, object>(propertySelection);
        }

        protected bool ClientRequestsChangeTo<TRoot, TProperty>(Expression<Func<TRoot, TProperty>> propertySelection)
        {
            var expression = propertySelection.Body;
            while (expression.NodeType == ExpressionType.Convert) //Called if implicit upcast is applied by the compiler
            {
                //Get the inner expression
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression.ToString() //the lambda body
                .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)  //We expect a property accessor devided by "."
                .Skip(1) // first segment is skipped (is the input parameter)
                .ToArray()
                .Transform(ClientRequestsChangeTo);
        }

        protected bool ClientRequestsChangeTo(params string[] expectedSectionKey)
        {
            var pathTokensToLeafLevel = expectedSectionKey.Take(Math.Max(0, expectedSectionKey.Length - 1)).ToArray(); //Find the base path on which the last property should exist
            var key = string.Join(".", pathTokensToLeafLevel);

            if (!_currentRequestProperties.TryGetValue(key, out var properties))
            {
                properties = _currentHttpRequest.GetDefinedJsonProperties(pathTokensToLeafLevel).ToHashSet(StringComparer.OrdinalIgnoreCase);
                _currentRequestProperties[key] = properties;
            }

            return properties.Contains(expectedSectionKey.Last());
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