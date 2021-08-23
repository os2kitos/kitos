using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItSystemUsageWriteModelMapperTest : WithAutoFixture
    {
        private readonly ItSystemUsageWriteModelMapper _sut;

        public ItSystemUsageWriteModelMapperTest()
        {
            _sut = new ItSystemUsageWriteModelMapper();
        }

        [Fact]
        public void Can_Map_Roles()
        {
            //Arrange
            var roles = Many<RoleAssignmentRequestDTO>().OrderBy(x => x.RoleUuid).ToList();

            //Act
            var systemUsageRoles = _sut.MapRoles(roles);

            //Assert
            var userRolePairs = AssertPropertyContainsDataChange(systemUsageRoles.UserRolePairs).OrderBy(x => x.RoleUuid).ToList();
            Assert.Equal(roles.Count, userRolePairs.Count);
            for (var i = 0; i < userRolePairs.Count; i++)
            {
                var expected = roles[i];
                var actual = userRolePairs[i];
                Assert.Equal(expected.RoleUuid, actual.RoleUuid);
                Assert.Equal(expected.UserUuid, actual.UserUuid);
            }
        }

        [Fact]
        public void Can_Map_ExternalReferences()
        {
            //Arrange
            var references = Many<ExternalReferenceDataDTO>().OrderBy(x => x.Url).ToList();

            //Act
            var mappedReferences = _sut.MapReferences(references).OrderBy(x => x.Url).ToList();

            //Assert
            Assert.Equal(mappedReferences.Count, mappedReferences.Count);
            for (var i = 0; i < mappedReferences.Count; i++)
            {
                var expected = references[i];
                var actual = mappedReferences[i];
                Assert.Equal(expected.Url, actual.Url);
                Assert.Equal(expected.Title, actual.Title);
                Assert.Equal(expected.DocumentId, actual.DocumentId);
                Assert.Equal(expected.MasterReference, actual.MasterReference);
            }
        }

        [Fact]
        public void Can_Map_KLE()
        {
            //Arrange
            var input = A<LocalKLEDeviationsRequestDTO>();

            //Act
            var output = _sut.MapKle(input);

            //Assert
            AssertKLE(input.AddedKLEUuids, output.AddedKLEUuids);
            AssertKLE(input.RemovedKLEUuids, output.RemovedKLEUuids);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Map_KLE_Resets_Property_If_Source_Is_Not_Defined(bool addedNull, bool removedNull)
        {
            //Arrange
            var input = A<LocalKLEDeviationsRequestDTO>();
            if (addedNull) input.AddedKLEUuids = null;
            if (removedNull) input.RemovedKLEUuids = null;

            //Act
            var output = _sut.MapKle(input);

            //Assert that null is translated into a reset value (change to "none")
            Assert.Equal(addedNull, output.AddedKLEUuids.Value.Value.IsNone);
            Assert.Equal(removedNull, output.RemovedKLEUuids.Value.Value.IsNone);
        }

        [Fact]
        public void Can_Map_OrganizationUsage()
        {
            //Arrange
            var input = A<OrganizationUsageWriteRequestDTO>();

            //Act
            var output = _sut.MapOrganizationalUsage(input);

            //Assert 
            var responsible = AssertPropertyContainsDataChange(output.ResponsibleOrganizationUnitUuid);
            var usingOrgUnits = AssertPropertyContainsDataChange(output.UsingOrganizationUnitUuids);
            Assert.Equal(input.ResponsibleOrganizationUnitUuid, responsible);
            Assert.Equal(input.UsingOrganizationUnitUuids, usingOrgUnits);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Map_OrganizationUsage_Resets_Property_If_Source_Value_Is_Not_Defined(bool responsibleIsNull, bool unitsAreNull)
        {
            //Arrange
            var input = A<OrganizationUsageWriteRequestDTO>();
            if (responsibleIsNull) input.ResponsibleOrganizationUnitUuid = null;
            if (unitsAreNull) input.UsingOrganizationUnitUuids = null;

            //Act
            var output = _sut.MapOrganizationalUsage(input);

            //Assert 
            if (responsibleIsNull)
                AssertPropertyContainsResetDataChange(output.ResponsibleOrganizationUnitUuid);
            else
                AssertPropertyContainsDataChange(output.ResponsibleOrganizationUnitUuid);
            if (unitsAreNull)
                AssertPropertyContainsResetDataChange(output.UsingOrganizationUnitUuids);
            else
                AssertPropertyContainsDataChange(output.UsingOrganizationUnitUuids);
        }

        [Fact]
        public void Can_Map_General_Data_Properties()
        {
            //Arrange
            var input = A<GeneralDataWriteRequestDTO>();

            //Act
            var output = _sut.MapGeneralData(input);

            //Assert
            Assert.Equal(input.LocalCallName, AssertPropertyContainsDataChange(output.LocalCallName));
            Assert.Equal(input.LocalSystemId, AssertPropertyContainsDataChange(output.LocalSystemId));
            Assert.Equal(input.SystemVersion, AssertPropertyContainsDataChange(output.SystemVersion));
            Assert.Equal(input.AssociatedProjectUuids, AssertPropertyContainsDataChange(output.AssociatedProjectUuids));
            Assert.Equal(input.DataClassificationUuid, AssertPropertyContainsDataChange(output.DataClassificationUuid));
            //TODO
            //Assert.Equal(input.Validity, AssertPropertyContainsDataChange(output.ValidFrom));
            Assert.Equal(input.Notes, AssertPropertyContainsDataChange(output.Notes));
         
            //TODO
            //Assert.Equal(input.NumberOfExpectedUsers, AssertPropertyContainsDataChange(output.));
        }

        private static T AssertPropertyContainsDataChange<T>(Maybe<ChangedValue<Maybe<T>>> sourceData)
        {
            Assert.True(sourceData.HasValue);
            Assert.True(sourceData.Value.Value.HasValue);
            return sourceData.Value.Value.Value;
        }

        private static T AssertPropertyContainsDataChange<T>(Maybe<ChangedValue<T>> sourceData)
        {
            Assert.True(sourceData.HasValue);
            return sourceData.Value.Value;
        }

        private static void AssertPropertyContainsResetDataChange<T>(Maybe<ChangedValue<Maybe<T>>> sourceData)
        {
            Assert.True(sourceData.HasValue);
            Assert.True(sourceData.Value.Value.IsNone);
        }

        private static void AssertKLE(IEnumerable<Guid> expected, Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>> actual)
        {
            Assert.True(actual.HasValue);
            Assert.True(actual.Value.Value.HasValue);
            var mappedUuids = actual.Value.Value.Value;
            Assert.Equal(expected, mappedUuids);
        }

        /*
         *
            UpdatedSystemUsageGeneralProperties MapGeneralData(GeneralDataWriteRequestDTO generalData);

            UpdatedSystemUsageGeneralProperties MapGeneralDataUpdate(GeneralDataUpdateRequestDTO generalData);

            UpdatedSystemUsageGDPRProperties MapGDPR(GDPRWriteRequestDTO request);
        
            UpdatedSystemUsageArchivingParameters MapArchiving(ArchivingWriteRequestDTO archiving);

            //Check that no reset is set
            SystemUsageUpdateParameters FromPOST(CreateItSystemUsageRequestDTO request);
            
            //Assert that sections are defined even if absent from the input
            SystemUsageUpdateParameters FromPUT(UpdateItSystemUsageRequestDTO request);
         */
    }
}
