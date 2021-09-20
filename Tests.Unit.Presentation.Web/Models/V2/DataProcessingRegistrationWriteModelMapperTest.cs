using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.Shared;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared.Write;
using Moq;
using Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class DataProcessingRegistrationWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly DataProcessingRegistrationWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;

        public DataProcessingRegistrationWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties()).Returns(GetAllRootProperties());
            _sut = new DataProcessingRegistrationWriteModelMapper(_currentHttpRequestMock.Object);
        }

        [Fact]
        public void MapGeneral_Returns_UpdatedDataProcessingRegistrationGeneralDataParameters()
        {
            //Arrange
            var input = A<DataProcessingRegistrationGeneralDataWriteRequestDTO>();

            //Act
            var output = _sut.MapGeneral(input);

            //Assert
            AssertGeneralData(input, output);
        }

        [Fact]
        public void MapGeneral__Resets_InsecureCountries_If_Input_Is_Null()
        {
            //Arrange
            var input = A<DataProcessingRegistrationGeneralDataWriteRequestDTO>();
            input.InsecureCountriesSubjectToDataTransferUuids = null;

            //Act
            var output = _sut.MapGeneral(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.InsecureCountriesSubjectToDataTransferUuids);
        }

        [Fact]
        public void MapGeneral__Resets_DataProcessors_If_Input_Is_Null()
        {
            //Arrange
            var input = A<DataProcessingRegistrationGeneralDataWriteRequestDTO>();
            input.DataProcessorUuids = null;

            //Act
            var output = _sut.MapGeneral(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.DataProcessorUuids);
        }

        [Fact]
        public void MapGeneral__Resets_SubDataProcessors_If_Input_Is_Null()
        {
            //Arrange
            var input = A<DataProcessingRegistrationGeneralDataWriteRequestDTO>();
            input.SubDataProcessorUuids = null;

            //Act
            var output = _sut.MapGeneral(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.SubDataProcessorUuids);
        }

        [Fact]
        public void FromPOST_Maps_All_Sections()
        {
            //Arrange
            var input = A<CreateDataProcessingRegistrationRequestDTO>();

            //Act
            var output = _sut.FromPOST(input);

            //Assert
            Assert.Equal(input.Name, AssertPropertyContainsDataChange(output.Name));
            AssertGeneralData(input.General, output.General.Value);
            AssertOversight(input.Oversight, AssertPropertyContainsDataChange(output.Oversight));
            AssertReferences(input.ExternalReferences.ToList(), AssertPropertyContainsDataChange(output.ExternalReferences).ToList());
            AssertRoles(input.Roles.ToList(), AssertPropertyContainsDataChange(output.Roles));
            Assert.Equal(input.SystemUsageUuids, output.SystemUsageUuids.Value);
        }

        [Fact]
        public void FromPUT_Maps_All_Sections()
        {
            //Arrange
            var input = A<UpdateDataProcessingRegistrationRequestDTO>();

            //Act
            var output = _sut.FromPUT(input);

            //Assert
            Assert.Equal(input.Name, AssertPropertyContainsDataChange(output.Name));
            AssertGeneralData(input.General, output.General.Value);
            AssertOversight(input.Oversight, AssertPropertyContainsDataChange(output.Oversight));
            AssertReferences(input.ExternalReferences.ToList(), AssertPropertyContainsDataChange(output.ExternalReferences).ToList());
            AssertRoles(input.Roles.ToList(), AssertPropertyContainsDataChange(output.Roles));
            Assert.Equal(input.SystemUsageUuids, output.SystemUsageUuids.Value);
        }

        [Fact]
        public void FromPATCH_Maps_All_Sections()
        {
            //Arrange
            var input = A<UpdateDataProcessingRegistrationRequestDTO>();

            //Act
            var output = _sut.FromPATCH(input);

            //Assert
            Assert.Equal(input.Name, AssertPropertyContainsDataChange(output.Name));
            AssertGeneralData(input.General, output.General.Value);
            AssertOversight(input.Oversight, AssertPropertyContainsDataChange(output.Oversight));
            AssertReferences(input.ExternalReferences.ToList(), AssertPropertyContainsDataChange(output.ExternalReferences).ToList());
            AssertRoles(input.Roles.ToList(), AssertPropertyContainsDataChange(output.Roles));
            Assert.Equal(input.SystemUsageUuids, output.SystemUsageUuids.Value);
        }

        public static IEnumerable<object[]> GetUndefinedSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(6);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPATCH_Ignores_Undefined_Sections(
            bool noName,
            bool noGeneralData,
            bool noSystems,
            bool noOversight,
            bool noRoles,
            bool noReferences)
        {
            //Arrange
            var input = new UpdateDataProcessingRegistrationRequestDTO();
            var properties = GetAllRootProperties();
            if (noName) properties.Remove(nameof(UpdateDataProcessingRegistrationRequestDTO.Name));
            if (noGeneralData) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.General));
            if (noSystems) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.SystemUsageUuids));
            if (noOversight) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.Oversight));
            if (noRoles) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.Roles));
            if (noReferences) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.ExternalReferences));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties()).Returns(properties);

            //Act
            var output = _sut.FromPATCH(input);

            //Assert that method patched empty values before mapping
            Assert.Equal(noName, output.Name.IsUnchanged);
            Assert.Equal(noGeneralData, output.General.IsNone);
            Assert.Equal(noSystems, output.SystemUsageUuids.IsNone);
            Assert.Equal(noOversight, output.Oversight.IsNone);
            Assert.Equal(noRoles, output.Roles.IsNone);
            Assert.Equal(noReferences, output.ExternalReferences.IsNone);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPUT_Enforces_Undefined_Sections(
            bool noName,
            bool noGeneralData,
            bool noSystems,
            bool noOversight,
            bool noRoles,
            bool noReferences)
        {
            //Arrange
            var input = new UpdateDataProcessingRegistrationRequestDTO();
            var properties = GetAllRootProperties();
            if (noName) properties.Remove(nameof(UpdateDataProcessingRegistrationRequestDTO.Name));
            if (noGeneralData) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.General));
            if (noSystems) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.SystemUsageUuids));
            if (noOversight) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.Oversight));
            if (noRoles) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.Roles));
            if (noReferences) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.ExternalReferences));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties()).Returns(properties);

            //Act
            var output = _sut.FromPUT(input);

            //Assert that method patched empty values before mapping
            Assert.True(output.Name.HasChange);
            Assert.True(output.General.HasValue);
            Assert.True(output.SystemUsageUuids.HasValue);
            Assert.True(output.Oversight.HasValue);
            Assert.True(output.Roles.HasValue);
            Assert.True(output.ExternalReferences.HasValue);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPOST_Ignores_Undefined_Sections(
           bool noName,
           bool noGeneralData,
           bool noSystems,
           bool noOversight,
           bool noRoles,
           bool noReferences)
        {
            //Arrange
            var input = new CreateDataProcessingRegistrationRequestDTO();
            var properties = GetAllRootProperties();
            if (noName) properties.Remove(nameof(CreateDataProcessingRegistrationRequestDTO.Name));
            if (noGeneralData) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.General));
            if (noSystems) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.SystemUsageUuids));
            if (noOversight) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.Oversight));
            if (noRoles) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.Roles));
            if (noReferences) properties.Remove(nameof(DataProcessingRegistrationWriteRequestDTO.ExternalReferences));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties()).Returns(properties);

            //Act
            var output = _sut.FromPOST(input);

            //Assert that method patched empty values before mapping
            Assert.Equal(noName, output.Name.IsUnchanged);
            Assert.Equal(noGeneralData, output.General.IsNone);
            Assert.Equal(noSystems, output.SystemUsageUuids.IsNone);
            Assert.Equal(noOversight, output.Oversight.IsNone);
            Assert.Equal(noRoles, output.Roles.IsNone);
            Assert.Equal(noReferences, output.ExternalReferences.IsNone);
        }

        [Fact]
        public void MapOversight_Returns_UpdatedDataProcessingRegistrationOversightDataParameters()
        {
            //Arrange
            var input = A<DataProcessingRegistrationOversightWriteRequestDTO>();

            //Act
            var output = _sut.MapOversight(input);

            //Assert
            AssertOversight(input, output);
        }

        [Fact]
        public void MapOversight_Resets_OversightOptions_If_Input_Is_Null()
        {
            //Arrange
            var input = A<DataProcessingRegistrationOversightWriteRequestDTO>();
            input.OversightOptionUuids = null;

            //Act
            var output = _sut.MapOversight(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.OversightOptionUuids);
        }

        [Fact]
        public void MapOversight_Resets_OversightDates_If_Input_Is_Null()
        {
            //Arrange
            var input = A<DataProcessingRegistrationOversightWriteRequestDTO>();
            input.OversightDates = null;

            //Act
            var output = _sut.MapOversight(input);

            //Assert
            AssertPropertyContainsResetDataChange(output.OversightDates);
        }

        [Fact]
        public void MapRoles_Returns_UpdatedDataProcessingRegistrationRoles()
        {
            //Arrange
            var roles = Many<RoleAssignmentRequestDTO>().OrderBy(x => x.RoleUuid).ToList();

            //Act
            var dprRoles = _sut.MapRoles(roles);

            //Assert
            AssertRoles(roles, dprRoles);
        }

        [Fact]
        public void Can_Map_ExternalReferences()
        {
            //Arrange
            var references = Many<ExternalReferenceDataDTO>().OrderBy(x => x.Url).ToList();

            //Act
            var mappedReferences = _sut.MapReferences(references).OrderBy(x => x.Url).ToList();

            //Assert
            AssertReferences(references, mappedReferences);
        }

        private static void AssertReferences(IReadOnlyList<ExternalReferenceDataDTO> references, IReadOnlyList<UpdatedExternalReferenceProperties> mappedReferences)
        {
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

        private static void AssertGeneralData(DataProcessingRegistrationGeneralDataWriteRequestDTO input, UpdatedDataProcessingRegistrationGeneralDataParameters output)
        {
            Assert.Equal(input.DataResponsibleUuid, AssertPropertyContainsDataChange(output.DataResponsibleUuid));
            Assert.Equal(input.DataResponsibleRemark, AssertPropertyContainsDataChange(output.DataResponsibleRemark));
            Assert.Equal(input.IsAgreementConcluded?.ToYesNoIrrelevantOption(), AssertPropertyContainsDataChange(output.IsAgreementConcluded));
            Assert.Equal(input.IsAgreementConcludedRemark, AssertPropertyContainsDataChange(output.IsAgreementConcludedRemark));
            Assert.Equal(input.AgreementConcludedAt, AssertPropertyContainsDataChange(output.AgreementConcludedAt));
            Assert.Equal(input.BasisForTransferUuid, AssertPropertyContainsDataChange(output.BasisForTransferUuid));
            Assert.Equal(input.TransferToInsecureThirdCountries?.ToYesNoUndecidedOption(), AssertPropertyContainsDataChange(output.TransferToInsecureThirdCountries));
            AssertNullableCollection(input, input.InsecureCountriesSubjectToDataTransferUuids, output.InsecureCountriesSubjectToDataTransferUuids);
            AssertNullableCollection(input, input.DataProcessorUuids, output.DataProcessorUuids);
            Assert.Equal(input.HasSubDataProcessors?.ToYesNoUndecidedOption(), AssertPropertyContainsDataChange(output.HasSubDataProcessors));
            AssertNullableCollection(input, input.SubDataProcessorUuids, output.SubDataProcessorUuids);
        }

        private static void AssertNullableCollection(DataProcessingRegistrationGeneralDataWriteRequestDTO input, IEnumerable<Guid> fromCollection, OptionalValueChange<Maybe<IEnumerable<Guid>>> actualCollection)
        {
            if (fromCollection == null)
                AssertPropertyContainsResetDataChange(actualCollection);
            else
                Assert.Equal(fromCollection,
                    AssertPropertyContainsDataChange(actualCollection));
        }

        private static void AssertOversightDates(IEnumerable<OversightDateDTO> expected, IEnumerable<UpdatedDataProcessingRegistrationOversightDate> actual)
        {
            var orderedExpected = expected.OrderBy(x => x.CompletedAt).ToList();
            var orderedActual = actual.OrderBy(x => x.CompletedAt).ToList();

            Assert.Equal(orderedExpected.Count, orderedActual.Count);
            for (var i = 0; i < orderedExpected.Count; i++)
            {
                Assert.Equal(orderedExpected[i].CompletedAt, orderedActual[i].CompletedAt);
                Assert.Equal(orderedExpected[i].Remark, orderedActual[i].Remark);
            }
        }

        private static HashSet<string> GetAllRootProperties()
        {
            return typeof(UpdateDataProcessingRegistrationRequestDTO).GetProperties().Select(x => x.Name)
                .ToHashSet();
        }

        private static void AssertOversight(DataProcessingRegistrationOversightWriteRequestDTO input,
            UpdatedDataProcessingRegistrationOversightDataParameters output)
        {
            Assert.Equal(input.OversightOptionUuids, AssertPropertyContainsDataChange(output.OversightOptionUuids));
            Assert.Equal(input.OversightOptionsRemark, AssertPropertyContainsDataChange(output.OversightOptionsRemark));
            Assert.Equal(input.OversightInterval?.ToIntervalOption(),
                AssertPropertyContainsDataChange(output.OversightInterval));
            Assert.Equal(input.OversightIntervalRemark, AssertPropertyContainsDataChange(output.OversightIntervalRemark));
            Assert.Equal(input.IsOversightCompleted?.ToYesNoUndecidedOption(),
                AssertPropertyContainsDataChange(output.IsOversightCompleted));
            Assert.Equal(input.OversightCompletedRemark, AssertPropertyContainsDataChange(output.OversightCompletedRemark));
            AssertOversightDates(input.OversightDates, AssertPropertyContainsDataChange(output.OversightDates));
        }

        private static void AssertRoles(IReadOnlyList<RoleAssignmentRequestDTO> expectedRoles, UpdatedDataProcessingRegistrationRoles actualRoles)
        {
            var inputRoles = expectedRoles.OrderBy(x => x.RoleUuid).ToList();
            var mappedRoles = AssertPropertyContainsDataChange(actualRoles.UserRolePairs).OrderBy(x => x.RoleUuid).ToList();
            Assert.Equal(expectedRoles.Count, mappedRoles.Count);
            for (var i = 0; i < mappedRoles.Count; i++)
            {
                var expected = inputRoles[i];
                var actual = mappedRoles[i];
                Assert.Equal(expected.RoleUuid, actual.RoleUuid);
                Assert.Equal(expected.UserUuid, actual.UserUuid);
            }
        }
    }
}
