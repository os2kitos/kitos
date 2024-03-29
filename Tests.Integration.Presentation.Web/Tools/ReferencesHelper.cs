﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class ReferencesHelper
    {
        public static async Task<ExternalReferenceDTO> CreateReferenceAsync(
            string title,
            string externalReferenceId,
            string referenceUrl,
            Action<ExternalReferenceDTO> setTargetId,
            Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/reference");

            var body = new ExternalReferenceDTO
            {
                Title = title,
                ExternalReferenceId = externalReferenceId,
                URL = referenceUrl
            };
            setTargetId(body);

            using var response = await HttpApi.PostWithCookieAsync(url, cookie, body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.ReadResponseBodyAsKitosApiResponseAsync<ExternalReferenceDTO>();
        }

        public static async Task<HttpResponseMessage> DeleteReferenceAsync(int id)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/reference/{id}?{KitosApiConstants.UnusedOrganizationIdParameter}");

            return await HttpApi.DeleteWithCookieAsync(url, cookie);
        }
    }
}
