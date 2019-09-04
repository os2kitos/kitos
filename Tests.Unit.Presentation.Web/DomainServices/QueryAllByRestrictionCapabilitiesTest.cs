using System;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Queries;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryAllByRestrictionCapabilitiesTest : WithAutoFixture
    {
        [Theory]
        [InlineData(typeof(ItSystem))]
        [InlineData(typeof(ItSystemUsage))]
        [InlineData(typeof(ItInterface))]
        [InlineData(typeof(ItContract))]
        [InlineData(typeof(ItProject))]
        [InlineData(typeof(Organization))]
        [InlineData(typeof(EconomyStream))]
        public void RequiresPostFiltering_Returns_False_Full_Cross_Organizational_Read_Access(Type type)
        {
            //Arrange
            var sut = CreateQuery(type, CrossOrganizationDataReadAccessLevel.All, A<int>());

            //Act
            bool result = sut.RequiresPostFiltering();

            //Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(typeof(ItSystem), CrossOrganizationDataReadAccessLevel.Public, false)]
        [InlineData(typeof(ItSystem), CrossOrganizationDataReadAccessLevel.None, false)]
        [InlineData(typeof(ItSystemUsage), CrossOrganizationDataReadAccessLevel.Public, false)]
        [InlineData(typeof(ItSystemUsage), CrossOrganizationDataReadAccessLevel.None, false)]
        [InlineData(typeof(ItInterface), CrossOrganizationDataReadAccessLevel.Public, false)]
        [InlineData(typeof(ItInterface), CrossOrganizationDataReadAccessLevel.None, false)]
        [InlineData(typeof(ItContract), CrossOrganizationDataReadAccessLevel.Public, false)]
        [InlineData(typeof(ItContract), CrossOrganizationDataReadAccessLevel.None, false)]
        [InlineData(typeof(ItProject), CrossOrganizationDataReadAccessLevel.Public, false)]
        [InlineData(typeof(ItProject), CrossOrganizationDataReadAccessLevel.None, false)]
        [InlineData(typeof(Organization), CrossOrganizationDataReadAccessLevel.Public, false)]
        [InlineData(typeof(EconomyStream), CrossOrganizationDataReadAccessLevel.Public, false)]

        //No IHasOrganization but has access modifier AND context aware AND sharing access is NONE and context aware does not support generic query since it only holds a method
        [InlineData(typeof(Organization), CrossOrganizationDataReadAccessLevel.None, true)]
        [InlineData(typeof(EconomyStream), CrossOrganizationDataReadAccessLevel.None, true)]

        public void RequiresPostFiltering_Returns(Type type, CrossOrganizationDataReadAccessLevel readAccess, bool expectedResult)
        {
            //Arrange
            var sut = CreateQuery(type, readAccess, A<int>());

            //Act
            bool result = sut.RequiresPostFiltering();

            //Assert
            Assert.Equal(expectedResult, result);
        }

        private static dynamic CreateQuery(Type type, CrossOrganizationDataReadAccessLevel readAccess, int organizationId)
        {
            var constructor =
                typeof(QueryAllByRestrictionCapabilities<>)
                    .MakeGenericType(type)
                    .GetConstructor(new[] {typeof(CrossOrganizationDataReadAccessLevel), typeof(int)});
            dynamic sut = constructor?.Invoke(new object[] {readAccess, organizationId});
            return sut;
        }
    }
}
