using Core.DomainModel.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model.Organizations
{
    public class OrganizationUnitTest : WithAutoFixture
    {
        /*[Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void GetAccessRights_CanBeRead(
            bool canBeRead,
            bool expectedResult)
        {
            //Arrange
            var unit = CreateOrganizationUnit();
            //Act
            var accessRights = unit.GetAccessRights(canBeRead, false, false);
            
            //Assert
            Assert.Equal(expectedResult, accessRights.CanBeRead);
            Assert.False(accessRights.CanBeModified);
            Assert.False(accessRights.CanNameBeModified);
            Assert.False(accessRights.CanBeRearranged);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void GetAccessRights_CanBeModified(
            bool canBeModified,
            bool expectedResult)
        {
            //Arrange
            var unit = CreateOrganizationUnit();

            //Act
            var accessRights = unit.GetAccessRights(false, canBeModified, false);

            //Assert
            Assert.Equal(expectedResult, accessRights.CanBeModified);
            Assert.False(accessRights.CanBeRead);
            Assert.False(accessRights.CanNameBeModified);
            Assert.False(accessRights.CanBeRearranged);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public void GetAccessRights_CanNameBeModified(
            bool isKitosUnit,
            bool canBeModified,
            bool expectedResult)
        {
            //Arrange
            var unit = CreateOrganizationUnit(isKitosUnit);
            //Act
            var accessRights = unit.GetAccessRights(false, canBeModified, false);

            //Assert
            Assert.Equal(expectedResult, accessRights.CanNameBeModified);
            Assert.Equal(canBeModified, accessRights.CanBeModified);
            Assert.False(accessRights.CanBeRead);
            Assert.False(accessRights.CanBeRearranged);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, true, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(true, true, false, false)]
        [InlineData(false, true, true, false)]
        [InlineData(true, true, true, true)]
        public void GetAccessRights_CanBeRearrangedModified(
            bool isKitosUnit, 
            bool hasParent,
            bool canBeModified,
            bool expectedResult)
        {
            //Arrange
            var unit = CreateOrganizationUnit(isKitosUnit, hasParent);
            //Act
            var accessRights = unit.GetAccessRights(false, canBeModified, false);

            //Assert
            Assert.Equal(expectedResult, accessRights.CanBeRearranged);
            Assert.Equal(canBeModified, accessRights.CanBeModified);
            Assert.Equal(canBeModified && isKitosUnit, accessRights.CanNameBeModified);
            Assert.False(accessRights.CanBeRead);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, true, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(true, true, false, false)]
        [InlineData(false, true, true, false)]
        [InlineData(true, true, true, true)]
        public void GetAccessRights_CanBeDeleted(
            bool isKitosUnit, 
            bool hasParent,
            bool canBeDeleted,
            bool expectedResult)
        {
            //Arrange
            var unit = CreateOrganizationUnit(isKitosUnit, hasParent);
            //Act
            var accessRights = unit.GetAccessRights(false, false, canBeDeleted);

            //Assert
            Assert.Equal(expectedResult, accessRights.CanBeDeleted);
            Assert.False(accessRights.CanBeRead);
            Assert.False(accessRights.CanBeModified);
            Assert.False(accessRights.CanNameBeModified);
            Assert.False(accessRights.CanBeRearranged);
        }

        public OrganizationUnit CreateOrganizationUnit(bool isKitosUnit = false, bool hasParent = false)
        {
            return new OrganizationUnit
            {
                Parent = hasParent ? new OrganizationUnit() : null,
                Origin = isKitosUnit ? OrganizationUnitOrigin.Kitos : OrganizationUnitOrigin.STS_Organisation
            };
        }*/
    }
}
