using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Core.ApplicationServices.Shared;
using Core.DomainModel.Organization;
using Infrastructure.Services.Extensions;
using Infrastructure.Services.Types;
using Tests.Integration.Presentation.Web.Tools;

namespace Tests.Integration.Presentation.Web.Constraints
{
    public class ApiPagingConstraintsTest
    {
        [Theory, MemberData(nameof(GetRESTApiInputs))]
        public async Task Paging_Constraints_Are_Applied_To_Token_Users_Through_GET_Requests_To_REST_API(string path, int pageSize, HttpStatusCode expectedStatusCode)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            //Act
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{path}?take={pageSize}"), token.Token);

            //Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory, MemberData(nameof(GetODATAInputs))]
        public async Task Paging_Constraints_Are_Applied_To_Token_Users_Through_GET_Requests_To_ODATA_API(string path, int pageSize, HttpStatusCode expectedStatusCode)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            //Act
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{path}?$top={pageSize}"), token.Token);

            //Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory, MemberData(nameof(GetODATAPathInputs))]
        public async Task OData_List_Requests_Must_Contain_Top(string path)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            //Act
            using var response = await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"{path}"), token.Token);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        public static IEnumerable<object[]> GetRESTApiInputs()
        {
            var resourceNames = new[]
            {
                "itsystem",
                "itsystemusage",
                "itcontract",
                "itproject",
            };

            return CreateInputs(resourceNames, false);
        }

        private static IEnumerable<object[]> CreateInputs(string[] resourceNames, bool odata)
        {
            var paths = CreatePaths(resourceNames, odata);

            foreach (var resource in paths)
            {
                yield return new object[] { resource, PagingContraints.MinPageSize, HttpStatusCode.OK };
                yield return new object[] { resource, PagingContraints.MaxPageSize, HttpStatusCode.OK };
                yield return new object[] { resource, PagingContraints.MinPageSize - 1, HttpStatusCode.BadRequest };
                yield return new object[] { resource, PagingContraints.MaxPageSize + 1, HttpStatusCode.BadRequest };
            }
        }

        private static IEnumerable<string> CreatePaths(IEnumerable<string> resourceNames, bool odata)
        {
            var apiType = odata ? "odata" : "api";
            var paths = resourceNames
                .Select(resource => $"{apiType}/{resource}")
                .ToList();
            return paths;
        }

        public static IEnumerable<object[]> GetODATAInputs()
        {
            var resourceNames = GetODataResourceNames();

            return CreateInputs(resourceNames.ToArray(), true);
        }

        public static IEnumerable<object[]> GetODATAPathInputs()
        {
            return
                GetODataResourceNames()
                    .Transform(names => CreatePaths(names, true))
                    .Transform(paths => paths.Select(path => new object[] { path }));
        }

        private static IEnumerable<string> GetODataResourceNames()
        {
            var resourceNames = new[]
            {
                "itsystems",
                "itsystemusages",
                "itcontracts",
                "itprojects",
            };
            return resourceNames;
        }
    }
}
