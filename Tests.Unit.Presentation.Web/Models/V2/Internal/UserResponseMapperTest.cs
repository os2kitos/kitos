using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Moq;
using Presentation.Web.Controllers.API.V2.Internal.Users.Mapping;
using Presentation.Web.Models.API.V2.Request.User;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2.Internal
{
    public class UserResponseMapperTest : WithAutoFixture
    {
        private readonly UserResponseModelMapper _sut;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        public UserResponseMapperTest()
        {
            _organizationServiceMock = new Mock<IOrganizationService>();
            _sut = new UserResponseModelMapper(_organizationServiceMock.Object);
        }

        [Fact]
        public void Can_Map_User_To_Dto()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var organization = new Organization
            {
                Uuid = orgUuid
            };
            var unit = new OrganizationUnit
            {
                Uuid = A<Guid>(),
                Name = A<string>()
            };
            var user = new User
            {
                Uuid = A<Guid>(),
                Email = A<string>(),
                Name = A<string>(),
                LastName = A<string>(),
                PhoneNumber = A<string>(),
                DefaultUserStartPreference = DefaultUserStartPreferenceChoiceMapper.GetDefaultUserStartPreferenceString(A<DefaultUserStartPreferenceChoice>()),
                HasApiAccess = A<bool>(),
                HasStakeHolderAccess = A<bool>(),
                OrganizationRights = new List<OrganizationRight>
                {
                    new()
                    {
                        Role = A<OrganizationRole>(),
                        Organization = new Organization{ Uuid = orgUuid} 
                    }
                }
            };

            _organizationServiceMock.Setup(x => x.GetOrganization(orgUuid, null)).Returns(organization);
            _organizationServiceMock.Setup(x => x.GetDefaultUnit(organization, user)).Returns(unit);

            //Act
            var result= _sut.ToUserResponseDTO(orgUuid, user);

            //Assert
            Assert.True(result.Ok);
            var response = result.Value;

            Assert.Equal(user.Uuid, response.Uuid);
            Assert.Equal(user.Email, response.Email);
            Assert.Equal(user.Name, response.FirstName);
            Assert.Equal(user.LastName, response.LastName);
            Assert.Equal(user.PhoneNumber, response.PhoneNumber);
            Assert.Equal(user.DefaultUserStartPreference, DefaultUserStartPreferenceChoiceMapper.GetDefaultUserStartPreferenceString(response.DefaultUserStartPreference));
            Assert.Equal(user.HasApiAccess, response.HasApiAccess);
            Assert.Equal(user.HasStakeHolderAccess, response.HasStakeHolderAccess);
            Assert.Equal(user.OrganizationRights.Select(x => x.Role.ToOrganizationRoleChoice()), response.Roles);
            Assert.Equal(unit.Uuid, response.DefaultOrganizationUnit.Uuid);
            Assert.Equal(unit.Name, response.DefaultOrganizationUnit.Name);
        }
    }
}
