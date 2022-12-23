using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Core.DomainServices.GDPR;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.Organization;
using Moq;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationDataProcessorAssignmentServiceTest : WithAutoFixture
    {
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly DataProcessingRegistrationDataProcessorAssignmentService _sut;
        private readonly Mock<IOptionsService<DataProcessingRegistration, DataProcessingCountryOption>> _countryOptionsServiceMock;
        private readonly Mock<IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption>> _basisForTransferOptionsServiceMock;

        public DataProcessingRegistrationDataProcessorAssignmentServiceTest()
        {
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _countryOptionsServiceMock = new Mock<IOptionsService<DataProcessingRegistration, DataProcessingCountryOption>>();
            _basisForTransferOptionsServiceMock = new Mock<IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption>>();
            _sut = new DataProcessingRegistrationDataProcessorAssignmentService(_organizationRepositoryMock.Object, _countryOptionsServiceMock.Object, _basisForTransferOptionsServiceMock.Object);
        }

        [Fact]
        public void GetApplicableDataProcessors_Returns_Organizations_Not_Already_Assigned()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var validCandidate1 = new Organization { Id = A<int>() };
            var validCandidate2 = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration() { DataProcessors = { existingProcessor } };

            ExpectGetAllOrganizationsReturn(validCandidate1, validCandidate2, existingProcessor);

            //Act
            var applicableDataProcessors = _sut.GetApplicableDataProcessors(dataProcessingRegistration).ToList();

            //Assert
            Assert.Equal(2, applicableDataProcessors.Count);
            Assert.Equal(new[] { validCandidate1, validCandidate2 }, applicableDataProcessors);
        }

        [Fact]
        public void Can_AssignDataProcessor()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var validCandidate = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration() { DataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(validCandidate.Id, validCandidate);

            //Act
            var result = _sut.AssignDataProcessor(dataProcessingRegistration, validCandidate.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(validCandidate, result.Value);
            Assert.True(dataProcessingRegistration.DataProcessors.Contains(validCandidate));
        }

        [Fact]
        public void Cannot_AssignDataProcessor_If_Not_Applicable()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var dpId = A<int>();
            var dataProcessingRegistration = new DataProcessingRegistration() { DataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(dpId, Maybe<Organization>.None);

            //Act
            var result = _sut.AssignDataProcessor(dataProcessingRegistration, dpId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_AssignDataProcessor_If_Already_Assigned()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration() { DataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(existingProcessor.Id, existingProcessor);

            //Act
            var result = _sut.AssignDataProcessor(dataProcessingRegistration, existingProcessor.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
        }

        [Fact]
        public void Can_RemoveDataProcessor()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration() { DataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(existingProcessor.Id, existingProcessor);

            //Act
            var result = _sut.RemoveDataProcessor(dataProcessingRegistration, existingProcessor.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(existingProcessor, result.Value);
            Assert.False(dataProcessingRegistration.DataProcessors.Contains(existingProcessor));
        }

        [Fact]
        public void Cannot_RemoveDataProcessor_If_Not_Applicable()
        {
            //Arrange
            var existingProcessor = new Organization { Id = A<int>() };
            var dpId = A<int>();
            var dataProcessingRegistration = new DataProcessingRegistration { DataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(dpId, Maybe<Organization>.None);

            //Act
            var result = _sut.RemoveDataProcessor(dataProcessingRegistration, dpId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_RemoveDataProcessor_If_Not_Present()
        {
            //Arrange
            var organization = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration();

            ExpectGetOrganizationByIdReturns(organization.Id, organization);

            //Act
            var result = _sut.RemoveDataProcessor(dataProcessingRegistration, organization.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void GetApplicableSubDataProcessors_Returns_Organizations_Not_Already_Assigned()
        {
            //Arrange
            var existingProcessor = CreateSubDataProcessor(new Organization { Id = A<int>() });
            var validCandidate1 = new Organization { Id = A<int>() };
            var validCandidate2 = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration() { AssignedSubDataProcessors = { existingProcessor } };

            ExpectGetAllOrganizationsReturn(validCandidate1, validCandidate2, existingProcessor.Organization);

            //Act
            var applicableSubDataProcessors = _sut.GetApplicableSubDataProcessors(dataProcessingRegistration).ToList();

            //Assert
            Assert.Equal(2, applicableSubDataProcessors.Count);
            Assert.Equal(new[] { validCandidate1, validCandidate2 }, applicableSubDataProcessors);
        }

        [Fact]
        public void Can_AssignSubDataProcessor()
        {
            //Arrange
            var existingProcessor = CreateSubDataProcessor(new Organization { Id = A<int>() });
            var validCandidate = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration() { AssignedSubDataProcessors = { existingProcessor }, HasSubDataProcessors = YesNoUndecidedOption.Yes };

            ExpectGetOrganizationByIdReturns(validCandidate.Id, validCandidate);

            //Act
            var result = _sut.AssignSubDataProcessor(dataProcessingRegistration, validCandidate.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(validCandidate, result.Value.Organization);
            Assert.True(dataProcessingRegistration.GetSubDataProcessor(validCandidate).HasValue);
        }

        [Fact]
        public void Cannot_AssignSubDataProcessor_If_Not_Using_Sub_Data_Processors()
        {
            //Arrange
            var existingProcessor = CreateSubDataProcessor(new Organization { Id = A<int>() });
            var validCandidate = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration() { AssignedSubDataProcessors = { existingProcessor }, HasSubDataProcessors = YesNoUndecidedOption.No };

            ExpectGetOrganizationByIdReturns(validCandidate.Id, validCandidate);

            //Act
            var result = _sut.AssignSubDataProcessor(dataProcessingRegistration, validCandidate.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_AssignSubDataProcessor_If_Not_Applicable()
        {
            //Arrange
            var existingProcessor = CreateSubDataProcessor(new Organization { Id = A<int>() });
            var dpId = A<int>();
            var dataProcessingRegistration = new DataProcessingRegistration() { AssignedSubDataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(dpId, Maybe<Organization>.None);

            //Act
            var result = _sut.AssignSubDataProcessor(dataProcessingRegistration, dpId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_AssignSubDataProcessor_If_Already_Assigned()
        {
            //Arrange
            var existingProcessor = CreateSubDataProcessor(new Organization { Id = A<int>() });
            var dataProcessingRegistration = new DataProcessingRegistration() { AssignedSubDataProcessors = { existingProcessor }, HasSubDataProcessors = YesNoUndecidedOption.Yes };

            ExpectGetOrganizationByIdReturns(existingProcessor.Id, existingProcessor.Organization);

            //Act
            var result = _sut.AssignSubDataProcessor(dataProcessingRegistration, existingProcessor.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void Can_UpdateSubDataProcessor(bool withBasisForTransfer, bool withTransferToInsecureThirdCountries, bool withInsecureThirdCountry)
        {
            //Arrange
            var existingProcessor = CreateSubDataProcessor(new Organization { Id = A<int>() });
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Organization = new Organization { Id = A<int>() },
                AssignedSubDataProcessors = { existingProcessor },
                HasSubDataProcessors = YesNoUndecidedOption.Yes
            };
            var basisForTransferOptionId = withBasisForTransfer ? A<int>() : (int?)null;
            var transferToInsecureThirdCountries = withTransferToInsecureThirdCountries
                ? YesNoUndecidedOption.Yes
                : EnumRange.AllExcept(YesNoUndecidedOption.Yes).RandomItem();
            var insecureThirdCountryOptionId = withInsecureThirdCountry ? A<int>() : (int?)null;
            var basisForTransferOption = new DataProcessingBasisForTransferOption();
            var countryOption = new DataProcessingCountryOption();

            ExpectGetOrganizationByIdReturns(existingProcessor.Organization.Id, existingProcessor.Organization);

            if (basisForTransferOptionId.HasValue)
                SetupResolveBasisForTransfer(dataProcessingRegistration, basisForTransferOptionId.Value, basisForTransferOption);

            if (insecureThirdCountryOptionId.HasValue)
                SetupResolveCountry(dataProcessingRegistration, insecureThirdCountryOptionId.Value, countryOption);

            //Act
            var result = _sut.UpdateSubDataProcessor(
                dataProcessingRegistration,
                existingProcessor.Organization.Id,
                basisForTransferOptionId,
                transferToInsecureThirdCountries,
                insecureThirdCountryOptionId);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(existingProcessor, result.Value);
            Assert.Equal(withBasisForTransfer ? basisForTransferOption : null, existingProcessor.SubDataProcessorBasisForTransfer);
            Assert.Equal(transferToInsecureThirdCountries, existingProcessor.TransferToInsecureCountry);
            Assert.Equal(withInsecureThirdCountry ? countryOption : null, existingProcessor.InsecureCountry);
        }

        [Fact]
        public void Cannot_UpdateSubDataProcessor_With_InsecureCountry_If_TransferToInsecureCountry_Has_Not_Been_Selected()
        {
            //Arrange
            var existingProcessor = CreateSubDataProcessor(new Organization { Id = A<int>() });
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Organization = new Organization { Id = A<int>() },
                AssignedSubDataProcessors = { existingProcessor },
                HasSubDataProcessors = YesNoUndecidedOption.Yes
            };
            var noTransfer = EnumRange.AllExcept(YesNoUndecidedOption.Yes).RandomItem();
            var insecureThirdCountryOptionId = A<int>();
            var countryOption = new DataProcessingCountryOption();

            ExpectGetOrganizationByIdReturns(existingProcessor.Organization.Id, existingProcessor.Organization);
            SetupResolveCountry(dataProcessingRegistration, insecureThirdCountryOptionId, countryOption);

            //Act
            var result = _sut.UpdateSubDataProcessor(
                dataProcessingRegistration,
                existingProcessor.Organization.Id,
                null,
                noTransfer,
                insecureThirdCountryOptionId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Can_UpdateSubDataProcessor_If_Option_Resolution_Fails(bool cannotResolveBasisForTransfer, bool cannotResolveCountry)
        {
            //Arrange
            var existingProcessor = CreateSubDataProcessor(new Organization { Id = A<int>() });
            var dataProcessingRegistration = new DataProcessingRegistration
            {
                Organization = new Organization { Id = A<int>() },
                AssignedSubDataProcessors = { existingProcessor },
                HasSubDataProcessors = YesNoUndecidedOption.Yes
            };
            var basisForTransferOptionId = A<int>();
            var transferToInsecureThirdCountries = YesNoUndecidedOption.Yes;
            var insecureThirdCountryOptionId = A<int>();
            var basisForTransferOption = new DataProcessingBasisForTransferOption();
            var countryOption = new DataProcessingCountryOption();
            ExpectGetOrganizationByIdReturns(existingProcessor.Organization.Id, existingProcessor.Organization);
            SetupResolveBasisForTransfer(dataProcessingRegistration, basisForTransferOptionId, cannotResolveBasisForTransfer ? Maybe<DataProcessingBasisForTransferOption>.None : basisForTransferOption);
            SetupResolveCountry(dataProcessingRegistration, insecureThirdCountryOptionId, cannotResolveCountry ? Maybe<DataProcessingCountryOption>.None : countryOption);

            //Act
            var result = _sut.UpdateSubDataProcessor(
                dataProcessingRegistration,
                existingProcessor.Organization.Id,
                basisForTransferOptionId,
                transferToInsecureThirdCountries,
                insecureThirdCountryOptionId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Can_RemoveSubDataProcessor()
        {
            //Arrange
            var existingProcessor = CreateSubDataProcessor(new Organization { Id = A<int>() });
            var dataProcessingRegistration = new DataProcessingRegistration() { AssignedSubDataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(existingProcessor.Id, existingProcessor.Organization);

            //Act
            var result = _sut.RemoveSubDataProcessor(dataProcessingRegistration, existingProcessor.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(existingProcessor, result.Value);
            Assert.False(dataProcessingRegistration.AssignedSubDataProcessors.Contains(existingProcessor));
        }

        [Fact]
        public void Cannot_RemoveSubDataProcessor_If_Not_Applicable()
        {
            //Arrange
            var existingProcessor = CreateSubDataProcessor(new Organization { Id = A<int>() });
            var dpId = A<int>();
            var dataProcessingRegistration = new DataProcessingRegistration { AssignedSubDataProcessors = { existingProcessor } };

            ExpectGetOrganizationByIdReturns(dpId, Maybe<Organization>.None);

            //Act
            var result = _sut.RemoveSubDataProcessor(dataProcessingRegistration, dpId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_RemoveSubDataProcessor_If_Not_Present()
        {
            //Arrange
            var organization = new Organization { Id = A<int>() };
            var dataProcessingRegistration = new DataProcessingRegistration();

            ExpectGetOrganizationByIdReturns(organization.Id, organization);

            //Act
            var result = _sut.RemoveSubDataProcessor(dataProcessingRegistration, organization.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        private void ExpectGetAllOrganizationsReturn(params Organization[] result)
        {
            _organizationRepositoryMock.Setup(x => x.GetAll())
                .Returns(result.AsQueryable());
        }

        private void ExpectGetOrganizationByIdReturns(int id, Maybe<Organization> validCandidate)
        {
            _organizationRepositoryMock.Setup(x => x.GetById(id)).Returns(validCandidate);
        }

        private SubDataProcessor CreateSubDataProcessor(Organization organization)
        {
            return new SubDataProcessor() { Organization = organization };
        }

        private void SetupResolveCountry(DataProcessingRegistration dataProcessingRegistration, int insecureThirdCountryOptionId, Maybe<DataProcessingCountryOption> result)
        {
            _countryOptionsServiceMock
                .Setup(x => x.GetAvailableOption(dataProcessingRegistration.Organization.Id,
                    insecureThirdCountryOptionId))
                .Returns(result);
        }

        private void SetupResolveBasisForTransfer(DataProcessingRegistration dataProcessingRegistration, int basisForTransferOptionId, Maybe<DataProcessingBasisForTransferOption> result)
        {
            _basisForTransferOptionsServiceMock
                .Setup(x => x.GetAvailableOption(dataProcessingRegistration.Organization.Id,
                    basisForTransferOptionId)).Returns(result);
        }
    }
}
