using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.GDPR;
using Core.DomainServices.Options;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationOversightOptionsAssignmentServiceTest : WithAutoFixture
    {
        private readonly Mock<IOptionsService<DataProcessingRegistration, DataProcessingOversightOption>> _optionsServiceMock;
        private readonly DataProcessingRegistrationOversightOptionsAssignmentService _sut;

        public DataProcessingRegistrationOversightOptionsAssignmentServiceTest()
        {
            _optionsServiceMock = new Mock<IOptionsService<DataProcessingRegistration, DataProcessingOversightOption>>();
            _sut = new DataProcessingRegistrationOversightOptionsAssignmentService(_optionsServiceMock.Object);
        }

        [Fact]
        public void Can_Assign()
        {
            //Arrange
            var registration = new DataProcessingRegistration
            {
                OrganizationId = A<int>()
            };
            var oversightOptionId = A<int>();
            var oversightOption = new DataProcessingOversightOption();
            ExpectGetAvailableOptionReturns(registration, oversightOptionId, oversightOption);

            //Act
            var result = _sut.Assign(registration, oversightOptionId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(oversightOption, result.Value);
            Assert.True(registration.OversightOptions.Contains(oversightOption));
        }

        [Fact]
        public void Cannot_Assign_If_OversightOption_Is_Not_Available()
        {
            //Arrange
            var registration = new DataProcessingRegistration
            {
                OrganizationId = A<int>()
            };
            var oversightOptionId = A<int>();
            ExpectGetAvailableOptionReturns(registration, oversightOptionId, Maybe<DataProcessingOversightOption>.None);

            //Act
            var result = _sut.Assign(registration, oversightOptionId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Assign_If_OversightOption_Is_Already_Assigned()
        {
            //Arrange
            var oversightOptionId = A<int>();
            var oversightOption = new DataProcessingOversightOption { Id = oversightOptionId };
            var registration = new DataProcessingRegistration
            {
                OrganizationId = A<int>(),
                OversightOptions = { oversightOption }
            };
            ExpectGetAvailableOptionReturns(registration, oversightOptionId, oversightOption);

            //Act
            var result = _sut.Assign(registration, oversightOptionId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
        }

        [Fact]
        public void Can_Remove()
        {
            //Arrange
            var oversightOptionId = A<int>();
            var oversightOption = new DataProcessingOversightOption { Id = oversightOptionId };
            var registration = new DataProcessingRegistration
            {
                OrganizationId = A<int>(),
                OversightOptions = { oversightOption }
            };
            ExpectGetOptionReturns(registration, oversightOptionId, oversightOption);

            //Act
            var result = _sut.Remove(registration, oversightOptionId);

            //Assert
            Assert.True(result.Ok);
            Assert.Empty(registration.OversightOptions);
        }

        [Fact]
        public void Cannot_Remove_If_OversightOption_Is_Invalid()
        {
            //Arrange
            var registration = new DataProcessingRegistration
            {
                OrganizationId = A<int>()
            };
            var oversightOptionId = A<int>();
            ExpectGetOptionReturns(registration, oversightOptionId, Maybe<DataProcessingOversightOption>.None);

            //Act
            var result = _sut.Remove(registration, oversightOptionId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        private void ExpectGetOptionReturns(DataProcessingRegistration registration, int oversightOptionId, Maybe<DataProcessingOversightOption> result)
        {
            _optionsServiceMock.Setup(x => x.GetOption(registration.OrganizationId, oversightOptionId)).Returns(result.Select(x => (x, true)));
        }

        private void ExpectGetAvailableOptionReturns(DataProcessingRegistration registration, int oversightOptionId, Maybe<DataProcessingOversightOption> result)
        {
            _optionsServiceMock.Setup(x => x.GetAvailableOption(registration.OrganizationId, oversightOptionId)).Returns(result);
        }
    }
}
