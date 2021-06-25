using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.UserQueries;
using System.Collections.Generic;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Users
{
    public class QueryByRightsholderAccessTest : WithAutoFixture
    {
        [Theory]
        [InlineData(OrganizationRole.ContractModuleAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin)]
        [InlineData(OrganizationRole.ProjectModuleAdmin)]
        [InlineData(OrganizationRole.ReportModuleAdmin)]
        [InlineData(OrganizationRole.SystemModuleAdmin)]
        [InlineData(OrganizationRole.User)]
        public void Apply_Returns_Users_With_Rightsholder_Access(OrganizationRole otherRole)
        {
            //Arrange
            var userWithRightsholder = new User() { 
                Id = A<int>(), 
                OrganizationRights = new List<OrganizationRight>(){ 
                    new OrganizationRight(){ Role = OrganizationRole.RightsHolderAccess }
                } 
            };
            var userWithMultipleRightsholder = new User() { 
                Id = A<int>(),
                OrganizationRights = new List<OrganizationRight>(){
                    new OrganizationRight(){ Role = OrganizationRole.RightsHolderAccess },
                    new OrganizationRight(){ Role = OrganizationRole.RightsHolderAccess }
                }
            };
            var userWithRightsholderAndNonRightsholder = new User()
            {
                Id = A<int>(),
                OrganizationRights = new List<OrganizationRight>(){
                    new OrganizationRight(){ Role = OrganizationRole.RightsHolderAccess },
                    new OrganizationRight(){ Role = otherRole }
                }
            };
            var userWithOtherThanRightsholder = new User() { 
                Id = A<int>(),
                OrganizationRights = new List<OrganizationRight>(){
                    new OrganizationRight(){ Role = otherRole }
                }
            };
            var userWithNothing = new User() { Id = A<int>() };

            var input = new[] { userWithRightsholder, userWithMultipleRightsholder, userWithRightsholderAndNonRightsholder, userWithOtherThanRightsholder, userWithNothing }.AsQueryable();
            var sut = new QueryByRightsholderAccess();

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.Equal(3, result.Count());

            var userWithRightsholderResult = result.First(x => x.Id == userWithRightsholder.Id);
            Assert.Collection(userWithRightsholderResult.OrganizationRights, 
                firstItem => Assert.Equal(OrganizationRole.RightsHolderAccess, firstItem.Role));

            var userWithMultipleRightsholderResult = result.First(x => x.Id == userWithMultipleRightsholder.Id);
            Assert.Collection(userWithMultipleRightsholderResult.OrganizationRights,
                firstItem => Assert.Equal(OrganizationRole.RightsHolderAccess, firstItem.Role),
                secondItem => Assert.Equal(OrganizationRole.RightsHolderAccess, secondItem.Role));

            var userWithRightsholderAndNonRightsholderResult = result.First(x => x.Id == userWithRightsholderAndNonRightsholder.Id);
            Assert.Collection(userWithRightsholderAndNonRightsholderResult.OrganizationRights,
                firstItem => Assert.Equal(OrganizationRole.RightsHolderAccess, firstItem.Role),
                secondItem => Assert.Equal(otherRole, secondItem.Role));
        }
    }
}
