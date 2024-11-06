using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public abstract class WriteModelMapperBase
    {
        private readonly ICurrentHttpRequest _currentHttpRequest;
        private readonly IDictionary<string, HashSet<string>> _currentRequestProperties;
        private readonly IDictionary<string, bool> _currentRequestResetSectionStatus;

        protected WriteModelMapperBase(ICurrentHttpRequest currentHttpRequest)
        {
            _currentHttpRequest = currentHttpRequest;
            _currentRequestProperties = new Dictionary<string, HashSet<string>>();
            _currentRequestResetSectionStatus = new Dictionary<string, bool>();
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
            return CreateChangeRule<TRoot>(false).MustUpdate(propertySelection);
        }

        protected IPropertyUpdateRule<TRoot> CreateChangeRule<TRoot>(bool enforceChangesAlways) => new MustUpdateIfDefinedOrEnforced<TRoot>(ClientRequestsChangeTo, enforceChangesAlways);

        private bool ClientRequestsChangeTo(params string[] expectedSectionKey)
        {
            string CreatePathKey(IEnumerable<string> strings)
            {
                var s = string.Join(".", strings);
                return s;
            }

            HashSet<string> UpdateProperties(IEnumerable<string> pathTokensToLeafLevel)
            {
                var key = CreatePathKey(pathTokensToLeafLevel);
                if (!_currentRequestProperties.TryGetValue(key, out var objectProperties))
                {
                    objectProperties = _currentHttpRequest.GetDefinedJsonProperties(pathTokensToLeafLevel)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                    _currentRequestProperties[key] = objectProperties;
                    if (objectProperties.Any())
                    {
                        //If it has properties, it is not reset
                        _currentRequestResetSectionStatus[key] = false;
                    }
                }

                return objectProperties;
            }

            var pathTokensToLeafLevel = expectedSectionKey.Take(Math.Max(0, expectedSectionKey.Length - 1)).ToList(); //Find the base path on which the last property should exist

            var properties = UpdateProperties(pathTokensToLeafLevel);

            if (expectedSectionKey.Any() && properties.Contains(expectedSectionKey.Last()))
            {
                return true;
            }

            //If the property was not defined see if a parent was defined and set the current level explicitly to null, which dictates a propagated reset
            var unCachedKeys = new List<string>();
            var isPartOfScopedReset = false;
            var tailIndex = pathTokensToLeafLevel.Count - 1;
            while (tailIndex > 0)
            {
                var previousSection = pathTokensToLeafLevel[tailIndex];
                var previousPath = pathTokensToLeafLevel.Take(tailIndex + 1).ToList();
                var currentKey = CreatePathKey(previousPath);
                if (_currentRequestResetSectionStatus.TryGetValue(currentKey, out var existingStatus))
                {
                    isPartOfScopedReset = existingStatus;
                    break;
                }
                unCachedKeys.Add(currentKey);

                //Check if the parent reset the section
                var parentPath = pathTokensToLeafLevel.Take(tailIndex);

                properties = UpdateProperties(parentPath);
                if (properties.Contains(previousSection))
                {
                    isPartOfScopedReset = _currentHttpRequest
                        .GetObject(previousPath)
                        .Select(x => x.Type == JTokenType.Null)//If the parent is set to null by the grand parent, then all items below the parent are also considered to be reset and hence part of the change set
                        .GetValueOrFallback(false);
                    break;
                }

                tailIndex--;
            }

            unCachedKeys.ForEach(k => _currentRequestResetSectionStatus[k] = isPartOfScopedReset);

            return isPartOfScopedReset;
        }

        protected IEnumerable<UpdatedExternalReferenceProperties> BaseMapCreateReferences(IEnumerable<ExternalReferenceDataWriteRequestDTO> references)
        {
            return references.Select(MapCommonReference).ToList();
        }

        protected IEnumerable<UpdatedExternalReferenceProperties> BaseMapUpdateReferences(IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> references)
        {
            return references.Select(MapUpdateReference).ToList();
        }

        protected static ChangedValue<Maybe<IEnumerable<UserRolePair>>> BaseMapRoleAssignments(IReadOnlyCollection<RoleAssignmentRequestDTO> roleAssignmentResponseDtos)
        {
            return (roleAssignmentResponseDtos.Any() ?
                Maybe<IEnumerable<UserRolePair>>.Some(roleAssignmentResponseDtos.Select(x => x.ToUserRolePair()).ToList()) :
                Maybe<IEnumerable<UserRolePair>>.None).AsChangedValue();
        }
        protected static UpdatedExternalReferenceProperties MapCommonReference<T>(T reference)
            where T : ExternalReferenceDataWriteRequestDTO
        {
            return new UpdatedExternalReferenceProperties(reference.Title, reference.DocumentId, reference.Url, reference.MasterReference);
        }

        protected OptionalValueChange<Maybe<T>> GetOptionalValueChange<T>(Func<bool> shouldUpdate, Maybe<T> dtoField)
        {
            return shouldUpdate()
                ? (dtoField ?? Maybe<T>.None).AsChangedValue()
                : OptionalValueChange<Maybe<T>>.None;
        }

        private static UpdatedExternalReferenceProperties MapUpdateReference(UpdateExternalReferenceDataWriteRequestDTO reference)
        {
            var updateProperties = MapCommonReference(reference);
            updateProperties.Uuid = reference.Uuid;

            return updateProperties;
        }
    }
}