using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class OrganizationHelper
    {
        public static async Task<ContactPersonDTO> GetContactPersonAsync(int organizationId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/contactPerson/{organizationId}"); //NOTE: This looks odd but it is how it works. On GET it uses the orgId and on patch it uses the contactPersonId
            using (var response = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ContactPersonDTO>();
            }
        }

        public static async Task<ContactPersonDTO> ChangeContactPersonAsync(int contactPersonId, int organizationId, string name, string lastName, string email, string phone, Cookie optionalLogin = null)
        {
            using (var createdResponse = await SendChangeContactPersonRequestAsync(contactPersonId, organizationId, name, lastName, email, phone, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ContactPersonDTO>();

                return response;
            }
        }

        public static async Task<HttpResponseMessage> SendChangeContactPersonRequestAsync(int contactPersonId, int organizationId, string name, string lastName, string email, string phone, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new
            {
                organizationId = organizationId,
                name = name,
                email = email,
                lastName = lastName,
                phoneNumber = phone
            };

            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/contactPerson/{contactPersonId}?organizationId={organizationId}"), cookie, body);
        }


        public static async Task<Core.DomainModel.Organization.Organization> CreateOrganizationAsync(int owningOrganizationId, int objectOwnerId, string name, string cvr, OrganizationTypeKeys type, AccessModifier accessModifier, Cookie optionalLogin = null)
        {
            using (var createdResponse = await SendCreateOrganizationRequestAsync(owningOrganizationId, objectOwnerId, name, cvr, type, accessModifier, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                return await createdResponse.ReadResponseBodyAsAsync<Core.DomainModel.Organization.Organization>();
            }
        }

        public static async Task<HttpResponseMessage> SendCreateOrganizationRequestAsync(int owningOrganizationId, int objectOwnerId, string name, string cvr, OrganizationTypeKeys type, AccessModifier accessModifier, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("odata/Organizations");

            var body = new
            {
                AccessModifier = ((int)accessModifier).ToString("D"),
                Cvr = cvr,
                Id = owningOrganizationId, //This looks odd, but is checked in BaseEntityController. Id is changed once created
                Name = name,
                ObjectOwnerId = objectOwnerId,
                TypeId = (int)type
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }
    }
}
