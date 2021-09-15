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
        [InlineData(false, false, false, false, false, false, false, false)]
		[InlineData(false, false, false, false, false, false, false, true)]
		[InlineData(false, false, false, false, false, false, true, false)]
		[InlineData(false, false, false, false, false, true, false, false)]
		[InlineData(false, false, false, false, true, false, false, false)]
		[InlineData(false, false, false, true, false, false, false, false)]
		[InlineData(false, false, true, false, false, false, false, false)]
		[InlineData(false, true, false, false, false, false, false, false)]
		[InlineData(true, false, false, false, false, false, false, false)]
		[InlineData(true, true, true, true, true, true, true, true)]
        public void FromPUT_Ignores_Undefined_Root_Sections(bool noName, bool noGeneralData, bool noParent, bool noResponsible, bool noProcurement, bool noSupplier, bool noHandoverTrials, bool noSystemUsages)
        {
            //Arrange
            var rootProperties = GetRootProperties();

            if (noName) rootProperties.Remove(nameof(UpdateContractRequestDTO.Name));
            if (noGeneralData) rootProperties.Remove(nameof(UpdateContractRequestDTO.General));
            if (noParent) rootProperties.Remove(nameof(UpdateContractRequestDTO.ParentContractUuid));
            if (noResponsible) rootProperties.Remove(nameof(UpdateContractRequestDTO.Responsible));
            if (noProcurement) rootProperties.Remove(nameof(UpdateContractRequestDTO.Procurement));
            if (noSupplier) rootProperties.Remove(nameof(UpdateContractRequestDTO.Supplier));
            if (noHandoverTrials) rootProperties.Remove(nameof(UpdateContractRequestDTO.HandoverTrials));
            if (noSystemUsages) rootProperties.Remove(nameof(UpdateContractRequestDTO.SystemUsageUuids));
			
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
            Assert.Equal(noSupplier, output.Supplier.IsNone);
            Assert.Equal(noHandoverTrials, output.HandoverTrials.IsNone);
            Assert.Equal(noSystemUsages, output.SystemUsageUuids.IsNone);
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

        [Fact]
        public void Can_Map_Supplier()
        {
            //Arrange
            var input = A<ContractSupplierDataWriteRequestDTO>();

            //Act
            var output = _sut.MapSupplier(input);

            //Assert
            AssertSupplier(input, output);
        }

        [Fact]
        public void Can_Map_Supplier_FromPOST()
        {
            //Arrange
            var input = A<ContractSupplierDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPOST(new CreateNewContractRequestDTO() { Supplier = input });

            //Assert
            AssertSupplier(input, output.Supplier.Value);
        }

        [Fact]
        public void Can_Map_Supplier_FromPUT()
        {
            //Arrange
            var input = A<ContractSupplierDataWriteRequestDTO>();

            //Act
            var output = _sut.FromPUT(new UpdateContractRequestDTO { Supplier = input });

            //Assert
            AssertSupplier(input, output.Supplier.Value);
        }

        [Fact]
        public void Can_Map_HandoverTrials()
        {
            //Arrange
            var input = Many<HandoverTrialRequestDTO>().ToList();

            //Act
            var output = _sut.MapHandOverTrials(input);

            //Assert
            AssertHandoverTrials(input, output);
        }

        [Fact]
        public void Can_Map_HandoverTrials_FromPOST()
        {
            //Arrange
            var input = Many<HandoverTrialRequestDTO>().ToList();

            //Act
            var output = _sut.FromPOST(new CreateNewContractRequestDTO() { HandoverTrials = input });

            //Assert
            AssertHandoverTrials(input, output.HandoverTrials.Value);
        }

        [Fact]
        public void Can_Map_HandoverTrials_FromPUT()
        {
            //Arrange
            var input = Many<HandoverTrialRequestDTO>().ToList();

            //Act
            var output = _sut.FromPUT(new UpdateContractRequestDTO { HandoverTrials = input });

            //Assert
            AssertHandoverTrials(input, output.HandoverTrials.Value);
        }

        private static void AssertHandoverTrials(List<HandoverTrialRequestDTO> input, IEnumerable<ItContractHandoverTrialUpdate> output)
        {
            var expected = input.OrderBy(x => x.HandoverTrialTypeUuid).ToList();
            var actual = output.OrderBy(x => x.HandoverTrialTypeUuid).ToList();
            Assert.Equal(expected.Count, actual.Count);
            foreach (var pair in expected.Zip(actual, (e, a) => new { inputDTO = e, mappedChange = a }))
            {
                Assert.Equal(pair.inputDTO.ApprovedAt, pair.mappedChange.ApprovedAt);
                Assert.Equal(pair.inputDTO.ExpectedAt, pair.mappedChange.ExpectedAt);
                Assert.Equal(pair.inputDTO.HandoverTrialTypeUuid, pair.mappedChange.HandoverTrialTypeUuid);
            }
        }

        private static void AssertSupplier(ContractSupplierDataWriteRequestDTO input, ItContractSupplierModificationParameters output)
        {
            Assert.Equal(input.OrganizationUuid, AssertPropertyContainsDataChange(output.OrganizationUuid));
            Assert.Equal(input.Signed, AssertPropertyContainsDataChange(output.Signed));
            Assert.Equal(input.SignedAt, AssertPropertyContainsDataChange(output.SignedAt));
            Assert.Equal(input.SignedBy, AssertPropertyContainsDataChange(output.SignedBy));
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_SystemUsages_From_Post(bool hasValues)
        {
            //Arrange
            var systemUsageUuids = hasValues ? new[] {A<Guid>(), A<Guid>()} : new Guid[0];
            var requestDto = new CreateNewContractRequestDTO { SystemUsageUuids = systemUsageUuids };

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.True(modificationParameters.SystemUsageUuids.HasValue);
            var modifiedUuids = modificationParameters.SystemUsageUuids.Value;
            AssertUuids(systemUsageUuids, modifiedUuids);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Map_SystemUsages_From_Put(bool hasValues)
        {
            //Arrange
            var systemUsageUuids = hasValues ? new[] { A<Guid>(), A<Guid>() } : new Guid[0];
            var requestDto = new UpdateContractRequestDTO { SystemUsageUuids = systemUsageUuids };

            //Act
            var modificationParameters = _sut.FromPUT(requestDto);

            //Assert
            Assert.True(modificationParameters.SystemUsageUuids.HasValue);
            var modifiedUuids = modificationParameters.SystemUsageUuids.Value;
            AssertUuids(systemUsageUuids, modifiedUuids);
        }

        private static void AssertUuids(IEnumerable<Guid> expected, IEnumerable<Guid> actual)
        {
            var orderedExpected = expected.OrderBy(x => x).ToList();
            var orderedActual = actual.OrderBy(x => x).ToList();

            Assert.Equal(orderedExpected.Count, orderedActual.Count);
            for (var i = 0; i < orderedExpected.Count; i++)
            {
                Assert.Equal(orderedExpected[i], orderedActual[i]);
            }
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
