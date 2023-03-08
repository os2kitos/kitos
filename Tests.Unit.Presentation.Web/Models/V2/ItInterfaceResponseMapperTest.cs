using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Model.Interface;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Interface;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItInterfaceResponseMapperTest : WithAutoFixture
    {
        private readonly ItInterfaceResponseMapper _sut;

        public ItInterfaceResponseMapperTest()
        {
            _sut = new ItInterfaceResponseMapper();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToItInterfaceResponseDTO_Returns_Mapped_ItInterfaces(bool minimumData)
        {
            //Arrange
            var itInterface = new ItInterface();
            itInterface.Name = A<string>();
            itInterface.Organization = new Organization() { Name = A<string>(), Cvr = A<string>() };
            if (!minimumData)
            {
                itInterface.ItInterfaceId = A<string>();
                itInterface.Version = A<string>();
                itInterface.Note = A<string>();
                itInterface.Disabled = A<bool>();
                itInterface.DataRows = new List<DataRow>()
                {
                    new() { Data = A<string>() },
                    new() { Data = A<string>(), DataType = new DataType() { Name = A<string>() } }
                };
                itInterface.Interface = new InterfaceType() { Name = A<string>() };
                itInterface.ExhibitedBy = new ItInterfaceExhibit() { ItSystem = new ItSystem() { Name = A<string>() } };
                itInterface.AccessModifier = A<AccessModifier>();
                itInterface.ObjectOwner = new User() { Name = A<string>(), LastName = A<string>() };
                itInterface.Created = A<DateTime>();
                itInterface.LastChanged = A<DateTime>();
                itInterface.LastChangedByUser = new User() { Name = A<string>(), LastName = A<string>() };
            }

            //Act
            var responseDto = _sut.ToItInterfaceResponseDTO(itInterface);

            //Assert
            Assert.Equal(itInterface.Uuid, responseDto.Uuid);
            Assert.Equal(itInterface.Name, responseDto.Name);
            Assert.Equal(itInterface.ItInterfaceId, responseDto.InterfaceId);
            Assert.Equal(itInterface.Version, responseDto.Version);
            Assert.Equal(itInterface.Url, responseDto.UrlReference);
            Assert.Equal(itInterface.AccessModifier.ToChoice(), responseDto.Scope);
            Assert.Equal(itInterface.Note, responseDto.Notes);
            Assert.Equal(itInterface.Created, responseDto.Created);
            AssertUser(itInterface.ObjectOwner, responseDto.CreatedBy);
            Assert.Equal(itInterface.LastChanged, responseDto.LastModified);
            AssertUser(itInterface.LastChangedByUser, responseDto.LastModifiedBy);
            Assert.Equal(itInterface.Disabled, responseDto.Deactivated);
            AssertIdentityReference(itInterface.Interface, responseDto.ItInterfaceType);
            AssertIdentityReference(itInterface.ExhibitedBy?.ItSystem, responseDto.ExposedBySystem);
            AssertOrganization(itInterface.Organization, responseDto.OrganizationContext);
            AssertData(itInterface, responseDto);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToRightsHolderItInterfaceResponseDTO_Returns_Mapped_RightsholderScoped_Response(bool minimumData)
        {
            //Arrange
            var itInterface = new ItInterface();
            itInterface.Name = A<string>();
            if (!minimumData)
            {
                itInterface.ItInterfaceId = A<string>();
                itInterface.Version = A<string>();
                itInterface.Note = A<string>();
                itInterface.Disabled = A<bool>();
                itInterface.ExhibitedBy = new ItInterfaceExhibit() { ItSystem = new ItSystem() { Name = A<string>() } };
                itInterface.ObjectOwner = new User() { Name = A<string>(), LastName = A<string>() };
                itInterface.Created = A<DateTime>();
            }

            //Act
            var responseDto = _sut.ToRightsHolderItInterfaceResponseDTO(itInterface);

            //Assert
            Assert.Equal(itInterface.Uuid, responseDto.Uuid);
            Assert.Equal(itInterface.Name, responseDto.Name);
            Assert.Equal(itInterface.ItInterfaceId, responseDto.InterfaceId);
            Assert.Equal(itInterface.Version, responseDto.Version);
            Assert.Equal(itInterface.Url, responseDto.UrlReference);
            Assert.Equal(itInterface.Note, responseDto.Notes);
            Assert.Equal(itInterface.Created, responseDto.Created);
            AssertUser(itInterface.ObjectOwner, responseDto.CreatedBy);
            Assert.Equal(itInterface.Disabled, responseDto.Deactivated);
            AssertIdentityReference(itInterface.ExhibitedBy?.ItSystem, responseDto.ExposedBySystem);
        }

        [Fact]
        public void Can_Map_Permissions()
        {
            //Arrange
            var itInterfacePermissions = A<ItInterfacePermissions>();

            //Act
            var dto = _sut.Map(itInterfacePermissions);

            //Assert
            Assert.Equal(itInterfacePermissions.BasePermissions.Delete,dto.Delete);
            Assert.Equal(itInterfacePermissions.BasePermissions.Modify,dto.Modify);
            Assert.Equal(itInterfacePermissions.BasePermissions.Read,dto.Read);
            Assert.Equivalent(itInterfacePermissions.DeletionConflicts.Select(x=>x.ToChoice()),dto.DeletionConflicts);
        }


        private static void AssertData(ItInterface expected, ItInterfaceResponseDTO actual)
        {
            var actualData = actual.Data.ToList();
            Assert.Equal(expected.DataRows.Count, actualData.Count);

            foreach (var dataRow in expected.DataRows)
            {
                var matchingRow = Assert.Single(actualData.Where(x => x.Uuid == dataRow.Uuid));
                AssertIdentityReference(dataRow.DataType, matchingRow.DataType);
                Assert.Equal(dataRow.Data, matchingRow.Description);
            }
        }

        private static void AssertUser(User expected, IdentityNamePairResponseDTO actual)
        {
            Assert.Equivalent(expected?.Transform(u => new IdentityNamePairResponseDTO(u.Uuid, u.GetFullName())), actual);
        }

        private static void AssertOrganization(Organization expected, ShallowOrganizationResponseDTO actual)
        {
            AssertIdentityReference(expected, actual);
            Assert.Equal(expected.GetActiveCvr(), actual.Cvr);
        }

        private static void AssertIdentityReference<T>(T expected, IdentityNamePairResponseDTO actual) where T : IHasName, IHasUuid
        {
            Assert.Equivalent(expected?.Transform(u => new IdentityNamePairResponseDTO(u.Uuid, u.Name)), actual);
        }
    }
}
