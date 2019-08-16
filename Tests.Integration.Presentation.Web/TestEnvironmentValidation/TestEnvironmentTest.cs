using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.TestEnvironmentValidation
{
    public class TestEnvironmentTest
    {
        [Theory]
        [InlineData(OrganizationRole.User)]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.User, true)]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        public void User_With_Role_Is_Available(OrganizationRole role, bool apiAccess = false)
        {
            var user = apiAccess ? TestEnvironment.GetCredentials(role, true) : TestEnvironment.GetCredentials(role);

            Assert.NotNull(user);
            Assert.False(string.IsNullOrWhiteSpace(user.Username));
            Assert.False(string.IsNullOrWhiteSpace(user.Password));
            Assert.Equal(role, user.Role);
        }
    }
}
