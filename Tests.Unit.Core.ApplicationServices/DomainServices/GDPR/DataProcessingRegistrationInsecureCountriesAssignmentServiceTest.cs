using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.GDPR;
using Core.DomainServices.Options;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationInsecureCountriesAssignmentServiceTest : WithAutoFixture
    {
        private readonly Mock<IOptionsService<DataProcessingRegistration, DataProcessingCountryOption>> _optionsServiceMock;
        private readonly DataProcessingRegistrationInsecureCountriesAssignmentService _sut;

        public DataProcessingRegistrationInsecureCountriesAssignmentServiceTest()
        {
            _optionsServiceMock = new Mock<IOptionsService<DataProcessingRegistration, DataProcessingCountryOption>>();
            _sut = new DataProcessingRegistrationInsecureCountriesAssignmentService(_optionsServiceMock.Object);
        }

        [Fact]
        public void Can_Assign()
        {
            //Arrange
            var registration = new DataProcessingRegistration
            {
                TransferToInsecureThirdCountries = YesNoUndecidedOption.Yes,
                OrganizationId = A<int>()
            };
            var countryId = A<int>();
            var countryOption = new DataProcessingCountryOption();
            ExpectGetAvailableOptionReturns(registration, countryId, countryOption);

            //Act
            var result = _sut.Assign(registration, countryId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(countryOption, result.Value);
            Assert.True(registration.InsecureCountriesSubjectToDataTransfer.Contains(countryOption));
        }

        [Fact]
        public void Cannot_Assign_If_Country_Is_Not_Available()
        {
            //Arrange
            var registration = new DataProcessingRegistration
            {
                TransferToInsecureThirdCountries = YesNoUndecidedOption.Yes,
                OrganizationId = A<int>()
            };
            var countryId = A<int>();
            ExpectGetAvailableOptionReturns(registration, countryId, Maybe<DataProcessingCountryOption>.None);

            //Act
            var result = _sut.Assign(registration, countryId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Assign_If_InsecureThirdCountryTransfer_Is_Not_Enabled()
        {
            //Arrange
            var registration = new DataProcessingRegistration
            {
                TransferToInsecureThirdCountries = YesNoUndecidedOption.No,
                OrganizationId = A<int>()
            };
            var countryId = A<int>();
            var countryOption = new DataProcessingCountryOption();
            ExpectGetAvailableOptionReturns(registration, countryId, countryOption);

            //Act
            var result = _sut.Assign(registration, countryId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Assign_If_Country_Is_Already_Assigned()
        {
            //Arrange
            var countryId = A<int>();
            var countryOption = new DataProcessingCountryOption { Id = countryId };
            var registration = new DataProcessingRegistration
            {
                TransferToInsecureThirdCountries = YesNoUndecidedOption.Yes,
                OrganizationId = A<int>(),
                InsecureCountriesSubjectToDataTransfer = { countryOption }
            };
            ExpectGetAvailableOptionReturns(registration, countryId, countryOption);

            //Act
            var result = _sut.Assign(registration, countryId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
        }

        [Fact]
        public void Can_Remove()
        {
            //Arrange
            var countryId = A<int>();
            var countryOption = new DataProcessingCountryOption { Id = countryId };
            var registration = new DataProcessingRegistration
            {
                OrganizationId = A<int>(),
                InsecureCountriesSubjectToDataTransfer = { countryOption }
            };
            ExpectGetOptionReturns(registration, countryId, countryOption);

            //Act
            var result = _sut.Remove(registration, countryId);

            //Assert
            Assert.True(result.Ok);
            Assert.Empty(registration.InsecureCountriesSubjectToDataTransfer);
        }

        [Fact]
        public void Cannot_Remove_If_CountryId_Is_Invalid()
        {
            //Arrange
            var countryId = A<int>();
            var registration = new DataProcessingRegistration
            {
                OrganizationId = A<int>()
            };
            ExpectGetOptionReturns(registration, countryId, Maybe<DataProcessingCountryOption>.None);

            //Act
            var result = _sut.Remove(registration, countryId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Assign_If_Country_Is_Not_Assigned()
        {
            //Arrange
            var countryId = A<int>();
            var countryOption = new DataProcessingCountryOption { Id = countryId };
            var registration = new DataProcessingRegistration
            {
                OrganizationId = A<int>(),
                InsecureCountriesSubjectToDataTransfer = { new DataProcessingCountryOption { Id = countryId + 1 } }
            };
            ExpectGetAvailableOptionReturns(registration, countryId, countryOption);

            //Act
            var result = _sut.Assign(registration, countryId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.NotEmpty(registration.InsecureCountriesSubjectToDataTransfer);
        }

        private void ExpectGetOptionReturns(DataProcessingRegistration registration, int countryId, Maybe<DataProcessingCountryOption> result)
        {
            _optionsServiceMock.Setup(x => x.GetOption(registration.OrganizationId, countryId)).Returns(result.Select(x => (x, true)));
        }

        private void ExpectGetAvailableOptionReturns(DataProcessingRegistration registration, int countryId, Maybe<DataProcessingCountryOption> result)
        {
            _optionsServiceMock.Setup(x => x.GetAvailableOption(registration.OrganizationId, countryId)).Returns(result);
        }
    }
}
