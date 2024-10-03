using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Internal.Users.Mapping;
using Presentation.Web.Models.API.V2.Request.User;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2.Internal
{
    public class UserResponseMapperTest : WithAutoFixture
    {
        private readonly UserResponseModelMapper _sut = new();

        [Fact]
        public void Can_Map_User_To_Dto()
        {
            //Arrange
            var orgUuid = A<Guid>();
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

            //Act
            var response = _sut.ToUserResponseDTO(orgUuid, user);

            //Assert
            Assert.Equal(user.Uuid, response.Uuid);
            Assert.Equal(user.Email, response.Email);
            Assert.Equal(user.Name, response.FirstName);
            Assert.Equal(user.LastName, response.LastName);
            Assert.Equal(user.PhoneNumber, response.PhoneNumber);
            Assert.Equal(user.DefaultUserStartPreference, DefaultUserStartPreferenceChoiceMapper.GetDefaultUserStartPreferenceString(response.DefaultUserStartPreference));
            Assert.Equal(user.HasApiAccess, response.HasApiAccess);
            Assert.Equal(user.HasStakeHolderAccess, response.HasStakeHolderAccess);
            Assert.Equal(user.OrganizationRights.Select(x => x.Role.ToOrganizationRoleChoice()), response.Roles);
        }
    }
}
