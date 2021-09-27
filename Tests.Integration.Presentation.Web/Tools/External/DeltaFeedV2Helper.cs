using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Response.Tracking;
using Presentation.Web.Models.API.V2.Types.Shared;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class DeltaFeedV2Helper
    {
        public static async Task<IEnumerable<TrackingEventResponseDTO>> GetDeletedEntitiesAsync(
        string token,
        TrackedEntityTypeChoice? entityType = null,
        DateTime? deletedSinceUTC = null,
        int? page = null,
        int? pageSize = null)
        {
            var path = "api/v2/delta-feed/deleted-entities";
            var queryParameters = new List<KeyValuePair<string, string>>();

            if (pageSize.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("pageSize", pageSize.Value.ToString("D")));

            if (page.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("page", page.Value.ToString("D")));

            if (deletedSinceUTC.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("deletedSinceUTC", deletedSinceUTC.Value.ToString("O")));

            if (entityType.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("entityType", entityType.Value.ToString("G")));

            if (queryParameters.Any())
                path += $"?{string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"))}";

            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(path), token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<TrackingEventResponseDTO>>();
        }
    }
}
