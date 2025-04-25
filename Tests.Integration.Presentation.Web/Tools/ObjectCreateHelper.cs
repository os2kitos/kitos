using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools.Model;
using AutoFixture;
using Presentation.Web.Models.API.V1;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class ObjectCreateHelper
    {
        private static readonly Fixture Fixture = new Fixture();

        public static LoginDTO MakeSimpleLoginDto(string email, string pwd)
        {
            return new()
            {
                Email = email,
                Password = pwd
            };
        }

        public static ApiUserDTO MakeSimpleApiUserDto(string email, bool apiAccess, bool stakeHolderAccess = false)
        {
            return new()
            {
                Email = email,
                Name = Fixture.Create<string>(),
                LastName = Fixture.Create<string>(),
                HasApiAccess = apiAccess,
                HasStakeHolderAccess = stakeHolderAccess,
            };
        }

        public static CreateUserDTO MakeSimpleCreateUserDto(ApiUserDTO apiUser)
        {
            return new()
            {
                user = apiUser,
                organizationId = TestEnvironment.DefaultOrganizationId,
                sendMailOnCreation = false
            };
        }
    }
}
