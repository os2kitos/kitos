using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using System.Collections.Generic;
using Tests.Toolkit.Patterns;
using System.Linq;
using Xunit;

namespace Tests.Unit.Core.Model
{
    public class ItInterfaceTest : WithAutoFixture
    {
        [Fact]
        public void UsedByOrganizationNames_Returns_OrganizationNames()
        {
            //Arrange
            var orgId1 = A<int>();
            var orgName1 = A<string>();
            var orgId2 = A<int>();
            var orgName2 = A<string>();
            var sut = new ItInterface()
            {
                AssociatedSystemRelations = new List<SystemRelation>()
                {
                    CreateSystemRelation(orgId1, orgName1),
                    CreateSystemRelation(orgId2, orgName2)
                }
            };

            //Act
            var orgNamesResult = sut.UsedByOrganizationNames;

            //Assert
            Assert.Equal(2, orgNamesResult.Count());
            Assert.Equal(orgName1, orgNamesResult.First(x => x.Equals(orgName1)));
            Assert.Equal(orgName2, orgNamesResult.First(x => x.Equals(orgName2)));
        }

        [Fact]
        public void UsedByOrganizationNames_Returns_OrganizationNames_Where_Ids_Is_Different_But_Name_Is_Same()
        {
            //Arrange
            var orgId1 = A<int>();
            var orgName = A<string>();
            var orgId2 = A<int>();
            var sut = new ItInterface()
            {
                AssociatedSystemRelations = new List<SystemRelation>()
                {
                    CreateSystemRelation(orgId1, orgName),
                    CreateSystemRelation(orgId2, orgName)
                }
            };

            //Act
            var orgNamesResult = sut.UsedByOrganizationNames;

            //Assert
            Assert.Equal(2, orgNamesResult.Count());
            Assert.Equal(orgName, orgNamesResult.First());
            Assert.Equal(orgName, orgNamesResult.Last());
        }

        [Fact]
        public void UsedByOrganizationNames_Returns_Distinct_OrganizationNames_If_Same_Organization_Is_In_Multiple_Relations()
        {
            //Arrange
            var orgId = A<int>();
            var orgName = A<string>();
            var sut = new ItInterface()
            {
                AssociatedSystemRelations = new List<SystemRelation>()
                {
                    CreateSystemRelation(orgId, orgName),
                    CreateSystemRelation(orgId, orgName)
                }
            };

            //Act
            var orgNamesResult = sut.UsedByOrganizationNames;

            //Assert
            var orgNameResult = Assert.Single(orgNamesResult);
            Assert.Equal(orgName, orgNameResult);
        }

        [Fact]
        public void UsedByOrganizationNames_Returns_Empty_List_If_No_Relations()
        {
            //Arrange
            var sut = new ItInterface()
            {
            };

            //Act
            var orgNamesResult = sut.UsedByOrganizationNames;

            //Assert
            Assert.Empty(orgNamesResult);
        }

        private SystemRelation CreateSystemRelation(int orgId, string orgName)
        {
            var fromSystemUsage = new ItSystemUsage()
            {
                Organization = new Organization()
                {
                    Id = orgId,
                    Name = orgName
                }
            };
            return new SystemRelation(fromSystemUsage);
        }
    }
}
