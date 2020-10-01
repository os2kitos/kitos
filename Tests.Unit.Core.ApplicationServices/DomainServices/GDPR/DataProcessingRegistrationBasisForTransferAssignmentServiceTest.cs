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
    public class DataProcessingRegistrationBasisForTransferAssignmentServiceTest : WithAutoFixture
    {
        private readonly Mock<IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption>> _optionsServiceMock;
        private readonly DataProcessingRegistrationBasisForTransferAssignmentService _sut;

        public DataProcessingRegistrationBasisForTransferAssignmentServiceTest()
        {
            _optionsServiceMock = new Mock<IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption>>();
            _sut = new DataProcessingRegistrationBasisForTransferAssignmentService(_optionsServiceMock.Object);
        }

        [Fact]
        public void Can_Assign()
        {
            //Arrange
            var dataProcessingRegistration = new DataProcessingRegistration { OrganizationId = A<int>() };
            var optionId = A<int>();
            var optionFromService = new DataProcessingBasisForTransferOption();
            _optionsServiceMock.Setup(x => x.GetAvailableOption(dataProcessingRegistration.OrganizationId, optionId)).Returns(optionFromService);

            //Act
            var result = _sut.Assign(dataProcessingRegistration, optionId);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(dataProcessingRegistration.BasisForTransfer, optionFromService);
        }

        [Fact]
        public void Cannot_Assign_If_Option_Is_Unavailable()
        {
            //Arrange
            var dataProcessingRegistration = new DataProcessingRegistration { OrganizationId = A<int>() };
            var optionId = A<int>();
            _optionsServiceMock.Setup(x => x.GetAvailableOption(dataProcessingRegistration.OrganizationId, optionId)).Returns(Maybe<DataProcessingBasisForTransferOption>.None);

            //Act
            var result = _sut.Assign(dataProcessingRegistration, optionId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Can_Clear()
        {
            //Arrange
            var dataProcessingRegistration = new DataProcessingRegistration { OrganizationId = A<int>(), BasisForTransfer = new DataProcessingBasisForTransferOption() };

            //Act
            var result = _sut.Clear(dataProcessingRegistration);

            //Assert
            Assert.True(result.Ok);
            Assert.Null(dataProcessingRegistration.BasisForTransfer);
        }

        [Fact]
        public void Cannot_Clear_If_Already_Cleared()
        {
            //Arrange
            var dataProcessingRegistration = new DataProcessingRegistration { OrganizationId = A<int>() };

            //Act
            var result = _sut.Clear(dataProcessingRegistration);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
        }
    }
}
