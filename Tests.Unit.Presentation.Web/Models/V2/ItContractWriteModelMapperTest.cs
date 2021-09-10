using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Contracts.Write;
using Moq;
using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Types.Contract;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItContractWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly ItContractWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;

        public ItContractWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties())
                .Returns(GetRootProperties());
            _sut = new ItContractWriteModelMapper(_currentHttpRequestMock.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test")]
        public void Can_Map_Name_From_Post(string name)
        {
            //Arrange
            var requestDto = new CreateNewContractRequestDTO { Name = name };

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test")]
        public void Can_Map_Name_From_Put(string name)
        {
            //Arrange
            var requestDto = new UpdateContractRequestDTO { Name = name };

            //Act
            var modificationParameters = _sut.FromPUT(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
        }

        [Theory]
        [InlineData(false, false, false, false,false)]
        [InlineData(false, false, false, false,true)]
        [InlineData(false, false, false, true, false)]
        [InlineData(false, false, true, false,false)]
        [InlineData(false, true, false, false,false)]
        [InlineData(true, false, false, false,false)]
        [InlineData(true, true, true, true, true)]
        public void FromPUT_Ignores_Undefined_Root_Sections(bool noName, bool noGeneralData, bool noParent, bool noResponsible,bool noProcurement)
        {
            //Arrange
            var rootProperties = GetRootProperties();

            if (noName) rootProperties.Remove(nameof(UpdateContractRequestDTO.Name));
            if (noGeneralData) rootProperties.Remove(nameof(UpdateContractRequestDTO.General));
            if (noParent) rootProperties.Remove(nameof(UpdateContractRequestDTO.ParentContractUuid));
            if (noResponsible) rootProperties.Remove(nameof(UpdateContractRequestDTO.Responsible));
            if (noProcurement) rootProperties.Remove(nameof(UpdateContractRequestDTO.Procurement));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties()).Returns(rootProperties);
            var emptyInput = new UpdateContractRequestDTO();

            //Act
            var output = _sut.FromPUT(emptyInput);

            //Assert
            Assert.Equal(noName, output.Name.IsUnchanged);
            Assert.Equal(noParent, output.ParentContractUuid.IsUnchanged);
            Assert.Equal(noGeneralData, output.General.IsNone);
            Assert.Equal(noResponsible, output.Responsible.IsNone);
            Assert.Equal(noProcurement, output.Procurement.IsNone);
        }

        [Fact]
        public void Can_Map_General()
        {
            //Arrange
            var input = A<ContractGeneralDataWriteRequestDTO>();

            //Act
            var output = _sut.MapGeneralData(input);

            //Assert
            AssertGeneralData(input, output);
        }

        [Fact]
        public void FromPost_Maps_General()
        {
            //Arrange
            var input = new CreateNewContractRequestDTO()
            {
                General = A<ContractGeneralDataWriteRequestDTO>()
            };

            //Act
            var output = _sut.FromPOST(input).General;

            //Assert
            AssertGeneralData(input.General, AssertPropertyContainsDataChange(output));
        }

        [Fact]
        public void FromPut_Maps_General()
        {
            //Arrange
            var input = new UpdateContractRequestDTO()
            {
                General = A<ContractGeneralDataWriteRequestDTO>()
            };

            //Act
            var output = _sut.FromPUT(input).General;

            //Assert
            AssertGeneralData(input.General, AssertPropertyContainsDataChange(output));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_Parent_From_Post(bool hasParentUuid)
        {
            //Arrange
            var parentUuid = hasParentUuid ? A<Guid?>() : null;
            var requestDto = new CreateNewContractRequestDTO { ParentContractUuid = parentUuid };

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.Equal(requestDto.ParentContractUuid, AssertPropertyContainsDataChange(modificationParameters.ParentContractUuid));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_Parent_From_Put(bool hasParentUuid)
        {
            //Arrange
            var parentUuid = hasParentUuid ? A<Guid?>() : null;
            var requestDto = new UpdateContractRequestDTO { ParentContractUuid = parentUuid };

            //Act
            var modificationParameters = _sut.FromPUT(requestDto);

            //Assert
            Assert.Equal(requestDto.ParentContractUuid, AssertPropertyContainsDataChange(modificationParameters.ParentContractUuid));
        }

        [Fact]
        public void Can_Map_Responsible()
        {
            //Arrange
            var input = A<ContractResponsibleDataWriteRequestDTO>();

            //Act
            var output = _sut.MapResponsible(input);

            //Assert
            AssertResponsible(input, output);
        }

        [Fact]
        public void Can_Map_Responsible_FromPOST()
        {
            //Arrange
            var input = A<ContractResponsibleDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPOST(new CreateNewContractRequestDTO() { Responsible = input });

            //Assert
            AssertResponsible(input, output.Responsible.Value);
        }

        [Fact]
        public void Can_Map_Responsible_FromPUT()
        {
            //Arrange
            var input = A<ContractResponsibleDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPUT(new UpdateContractRequestDTO { Responsible = input });

            //Assert
            AssertResponsible(input, output.Responsible.Value);
        }

        private static void AssertResponsible(ContractResponsibleDataWriteRequestDTO input,
            ItContractResponsibleDataModificationParameters output)
        {
            Assert.Equal(input.OrganizationUnitUuid, AssertPropertyContainsDataChange(output.OrganizationUnitUuid));
            Assert.Equal(input.Signed, AssertPropertyContainsDataChange(output.Signed));
            Assert.Equal(input.SignedAt, AssertPropertyContainsDataChange(output.SignedAt));
            Assert.Equal(input.SignedBy, AssertPropertyContainsDataChange(output.SignedBy));
		}
		
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_Procurement_From_Post(bool hasValues)
        {
            //Arrange
            var procurement = CreateProcurementRequest(hasValues);
            var requestDto = new CreateNewContractRequestDTO { Procurement = procurement };

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.True(modificationParameters.Procurement.HasValue);
            var procurementDto = modificationParameters.Procurement.Value;
            AssertProcurement(hasValues, procurement, procurementDto);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_Procurement_From_Put(bool hasValues)
        {
            //Arrange
            var procurement = CreateProcurementRequest(hasValues);
            var requestDto = new UpdateContractRequestDTO { Procurement = procurement };

            //Act
            var modificationParameters = _sut.FromPUT(requestDto);

            //Assert
            Assert.True(modificationParameters.Procurement.HasValue);
            var procurementDto = modificationParameters.Procurement.Value;
            AssertProcurement(hasValues, procurement, procurementDto);
        }

        [Fact]
        public void Can_Map_Procurement()
        {
            //Arrange
            var input = A<ContractProcurementDataWriteRequestDTO>();

            //Act
            var output = _sut.MapProcurement(input);

            //Assert
            AssertProcurement(true, input, output);
        }

        private static void AssertGeneralData(ContractGeneralDataWriteRequestDTO input,
            ItContractGeneralDataModificationParameters output)
        {
            Assert.Equal(input.ContractId, AssertPropertyContainsDataChange(output.ContractId));
            Assert.Equal(input.ContractTypeUuid, AssertPropertyContainsDataChange(output.ContractTypeUuid));
            Assert.Equal(input.ContractTemplateUuid, AssertPropertyContainsDataChange(output.ContractTemplateUuid));
            Assert.Equal(input.AgreementElementUuids, AssertPropertyContainsDataChange(output.AgreementElementUuids));
            Assert.Equal(input.Notes, AssertPropertyContainsDataChange(output.Notes));
            Assert.Equal(input.Validity.ValidFrom, AssertPropertyContainsDataChange(output.ValidFrom));
            Assert.Equal(input.Validity.ValidTo, AssertPropertyContainsDataChange(output.ValidTo));
            Assert.Equal(input.Validity.EnforcedValid, AssertPropertyContainsDataChange(output.EnforceValid));
        }

        private static void AssertProcurement(bool hasValues, ContractProcurementDataWriteRequestDTO expected, ItContractProcurementModificationParameters actual)
        {
            Assert.Equal(expected.ProcurementStrategyUuid, AssertPropertyContainsDataChange(actual.ProcurementStrategyUuid));
            Assert.Equal(expected.PurchaseTypeUuid, AssertPropertyContainsDataChange(actual.PurchaseTypeUuid));
            if (hasValues)
            {
                var (half, year) = AssertPropertyContainsDataChange(actual.ProcurementPlan);
                Assert.Equal(expected.ProcurementPlan.HalfOfYear, half);
                Assert.Equal(expected.ProcurementPlan.Year, year);
            }
            else
            {
                AssertPropertyContainsResetDataChange(actual.ProcurementPlan);
            }
        }

        private ContractProcurementDataWriteRequestDTO CreateProcurementRequest(bool hasValues)
        {
            return new ContractProcurementDataWriteRequestDTO
            {
                ProcurementStrategyUuid = hasValues ? A<Guid>() : null,
                PurchaseTypeUuid = hasValues ? A<Guid>() : null,
                ProcurementPlan = hasValues
                    ? new ProcurementPlanDTO()
                    {
                        HalfOfYear = Convert.ToByte(A<int>() % 1 + 1),
                        Year = A<int>()
                    }
                    : null
            };
        }

        private static HashSet<string> GetRootProperties()
        {
            return typeof(CreateNewContractRequestDTO).GetProperties().Select(x => x.Name).ToHashSet();
        }
    }
}
