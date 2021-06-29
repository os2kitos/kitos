using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.UserQueries;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Users
{
    public class QueryByRoleAssignmentTest : WithAutoFixture
    {
        [Theory, MemberData(nameof(GetAllRolesInput))]
        public void Apply_Returns_Users_With_Requested_Role(OrganizationRole searchForRole)
        {
            //Arrange
            var matchedUser1 = CreateUser(GetRandomRole(searchForRole), searchForRole);
            var matchedUser2 = CreateUser(searchForRole);
            var unMatchedUser = CreateUser(GetRandomRole(searchForRole), GetRandomRole(searchForRole));
            var input = new[] { matchedUser1, unMatchedUser, matchedUser2 }.AsQueryable();
            var sut = new QueryByRoleAssignment(searchForRole);

            //Act
            var result = sut.Apply(input).ToList();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, user => user == matchedUser1);
            Assert.Contains(result, user => user == matchedUser2);
        }

        private static User CreateUser(params OrganizationRole[] roles)
        {
            User user = new();

            foreach (var organizationRole in roles)
                user.OrganizationRights.Add(new OrganizationRight { Role = organizationRole });

            return user;
        }

        private OrganizationRole GetRandomRole(OrganizationRole exceptRole)
        {
            var random = new Random(A<int>());
            return GetAllRoles().OrderBy(_ => random.Next()).Where(role => role != exceptRole).First();
        }

        public static IEnumerable<OrganizationRole> GetAllRoles() => Enum.GetValues(typeof(OrganizationRole)).Cast<OrganizationRole>();
        public static IEnumerable<object[]> GetAllRolesInput() => GetAllRoles().Select(role => new object[] { role });
    }
}
