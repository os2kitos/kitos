using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.GDPR.Write;
using Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Types.DataProcessing;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class DataProcessingRegistrationWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly DataProcessingRegistrationWriteModelMapper _sut;

        public DataProcessingRegistrationWriteModelMapperTest()
        {
            _sut = new DataProcessingRegistrationWriteModelMapper();
        }

        [Fact]
        public void MapGeneral_Returns_UpdatedDataProcessingRegistrationGeneralDataParameters()
        {
            //Arrange
            var input = A<DataProcessingRegistrationGeneralDataWriteRequestDTO>();

            //Act
            var output = _sut.MapGeneral(input);

            //Assert
            Assert.Equal(input.DataResponsibleUuid,AssertPropertyContainsDataChange(output.DataResponsibleUuid));
            Assert.Equal(input.DataResponsibleRemark,AssertPropertyContainsDataChange(output.DataResponsibleRemark));
            Assert.Equal(input.IsAgreementConcluded?.ToYesNoIrrelevantOption(),AssertPropertyContainsDataChange(output.IsAgreementConcluded));
            Assert.Equal(input.IsAgreementConcludedRemark,AssertPropertyContainsDataChange(output.IsAgreementConcludedRemark));
            Assert.Equal(input.AgreementConcludedAt,AssertPropertyContainsDataChange(output.AgreementConcludedAt));
            Assert.Equal(input.BasisForTransferUuid,AssertPropertyContainsDataChange(output.BasisForTransferUuid));
            Assert.Equal(input.TransferToInsecureThirdCountries?.ToYesNoUndecidedOption(),AssertPropertyContainsDataChange(output.TransferToInsecureThirdCountries));
            Assert.Equal(input.InsecureCountriesSubjectToDataTransferUuids,AssertPropertyContainsDataChange(output.InsecureCountriesSubjectToDataTransferUuids));
            Assert.Equal(input.DataProcessorUuids,AssertPropertyContainsDataChange(output.DataProcessorUuids));
            Assert.Equal(input.HasSubDataProcessors?.ToYesNoUndecidedOption(),AssertPropertyContainsDataChange(output.HasSubDataProcessors));
            Assert.Equal(input.SubDataProcessorUuids,AssertPropertyContainsDataChange(output.SubDataProcessorUuids));
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
        public void MapOversight_Returns_UpdatedDataProcessingRegistrationOversightDataParameters()
        {
            //Arrange
            var input = A<DataProcessingRegistrationOversightWriteRequestDTO>();

            //Act
            var output = _sut.MapOversight(input);

            //Assert
            Assert.Equal(input.OversightOptionUuids, AssertPropertyContainsDataChange(output.OversightOptionUuids));
            Assert.Equal(input.OversightOptionsRemark, AssertPropertyContainsDataChange(output.OversightOptionsRemark));
            Assert.Equal(input.OversightInterval?.ToIntervalOption(), AssertPropertyContainsDataChange(output.OversightInterval));
            Assert.Equal(input.OversightIntervalRemark, AssertPropertyContainsDataChange(output.OversightIntervalRemark));
            Assert.Equal(input.IsOversightCompleted?.ToYesNoUndecidedOption(), AssertPropertyContainsDataChange(output.IsOversightCompleted));
            Assert.Equal(input.OversightCompletedRemark, AssertPropertyContainsDataChange(output.OversightCompletedRemark));
            AssertOversightDates(input.OversightDates, AssertPropertyContainsDataChange(output.OversightDates));
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

        private void AssertOversightDates(IEnumerable<OversightDateDTO> expected, IEnumerable<UpdatedDataProcessingRegistrationOversightDate> actual)
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
    }
}
