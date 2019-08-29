using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools.Model;
using AutoFixture;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class ObjectCreateHelper
    {
        private static readonly Fixture Fixture = new Fixture();

        public static LoginDTO MakeSimpleLoginDto(string email, string pwd)
        {
            return new LoginDTO
            {
                Email = email,
                Password = pwd
            };
        }

        public static ApiUserDTO MakeSimpleApiUserDto(string email, bool apiAccess)
        {
            return new ApiUserDTO
            {
                Email = email,
                Name = Fixture.Create<string>(),
                LastName = Fixture.Create<string>(),
                HasApiAccess = apiAccess
            };
        }

        public static CreateUserDTO MakeSimpleCreateUserDto(ApiUserDTO apiUser)
        {
            return new CreateUserDTO
            {
                user = apiUser,
                organizationId = TestEnvironment.DefaultOrganizationId,
                sendMailOnCreation = false
            };
        }
    }
}
