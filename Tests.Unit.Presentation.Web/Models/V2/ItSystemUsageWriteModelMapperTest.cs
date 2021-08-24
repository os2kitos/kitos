using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.SystemUsage.Write;
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

        //TODO: Opret y type: OptionalPropertyChange<T> => Value = Maybe<ChangeValue<T>> State, Match etc, PropertyChanged -> State.IsSome. Vil gørede der objekter mere læsbare

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
            AssertCommonGeneralDataWriteProperties(input, output);
            Assert.True(output.MainContractUuid.IsNone, "The main contract should be untouched as it is not part of the initial write contract");
        }

        [Fact]
        public void Map_General_Data_Properties_Resets_Validity_If_SourceValue_Not_Defined()
        {
            //Arrange
            var input = A<GeneralDataWriteRequestDTO>();
            input.Validity = null;

            //Act
            var output = _sut.MapGeneralData(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.EnforceActive);
            AssertPropertyContainsResetDataChange(output.ValidFrom);
            AssertPropertyContainsResetDataChange(output.ValidTo);
        }

        [Fact]
        public void Map_General_Data_Properties_Resets_NumberOfExpectedUsers_If_SourceValue_Not_Defined()
        {
            //Arrange
            var input = A<GeneralDataWriteRequestDTO>();
            input.NumberOfExpectedUsers = null;

            //Act
            var output = _sut.MapGeneralData(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.NumberOfExpectedUsersInterval);
        }

        [Fact]
        public void Map_General_Data_Properties_Resets_AssociatedProjects_If_SourceValue_Not_Defined()
        {
            //Arrange
            var input = A<GeneralDataWriteRequestDTO>();
            input.AssociatedProjectUuids = null;

            //Act
            var output = _sut.MapGeneralData(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.AssociatedProjectUuids);
        }

        [Fact]
        public void Can_Map_General_Data_Update_Properties()
        {
            //Arrange
            var input = A<GeneralDataUpdateRequestDTO>();

            //Act
            var output = _sut.MapGeneralDataUpdate(input);

            //Assert
            AssertCommonGeneralDataWriteProperties(input, output);
            Assert.Equal(input.MainContractUuid, AssertPropertyContainsDataChange(output.MainContractUuid));
        }

        [Fact]
        public void Map_General_Data_Update_Properties_Resets_Main_Contract_Id_If_Undefined()
        {
            //Arrange
            var input = A<GeneralDataUpdateRequestDTO>();
            input.MainContractUuid = null;

            //Act
            var output = _sut.MapGeneralDataUpdate(input);

            //Assert
            AssertCommonGeneralDataWriteProperties(input, output);
            AssertPropertyContainsResetDataChange(output.MainContractUuid);
        }

        private static void AssertCommonGeneralDataWriteProperties(GeneralDataWriteRequestDTO input,
            UpdatedSystemUsageGeneralProperties output)
        {
            Assert.Equal(input.LocalCallName, AssertPropertyContainsDataChange(output.LocalCallName));
            Assert.Equal(input.LocalSystemId, AssertPropertyContainsDataChange(output.LocalSystemId));
            Assert.Equal(input.SystemVersion, AssertPropertyContainsDataChange(output.SystemVersion));
            Assert.Equal(input.AssociatedProjectUuids, AssertPropertyContainsDataChange(output.AssociatedProjectUuids));
            Assert.Equal(input.DataClassificationUuid, AssertPropertyContainsDataChange(output.DataClassificationUuid));
            Assert.Equal(input.Validity?.EnforcedValid, AssertPropertyContainsDataChange(output.EnforceActive));
            Assert.Equal(input.Validity?.ValidFrom, AssertPropertyContainsDataChange(output.ValidFrom));
            Assert.Equal(input.Validity?.ValidTo, AssertPropertyContainsDataChange(output.ValidTo));
            Assert.Equal(input.Notes, AssertPropertyContainsDataChange(output.Notes));
            Assert.Equal(input.NumberOfExpectedUsers.LowerBound,
                AssertPropertyContainsDataChange(output.NumberOfExpectedUsersInterval).lower);
            Assert.Equal(input.NumberOfExpectedUsers.UpperBound,
                AssertPropertyContainsDataChange(output.NumberOfExpectedUsersInterval).upperBound);
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
            UpdatedSystemUsageGDPRProperties MapGDPR(GDPRWriteRequestDTO request);
        
            UpdatedSystemUsageArchivingParameters MapArchiving(ArchivingWriteRequestDTO archiving);

            //Check that no reset is set
            SystemUsageUpdateParameters FromPOST(CreateItSystemUsageRequestDTO request);
            
            //Assert that sections are defined even if absent from the input
            SystemUsageUpdateParameters FromPUT(UpdateItSystemUsageRequestDTO request);
         */
    }
}
