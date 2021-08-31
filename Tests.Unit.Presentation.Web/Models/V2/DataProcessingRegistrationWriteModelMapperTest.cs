using Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
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
            Assert.Equal(input.HasSubDataProcesors?.ToYesNoUndecidedOption(),AssertPropertyContainsDataChange(output.HasSubDataProcesors));
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
    }
}
