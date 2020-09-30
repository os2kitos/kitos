using System;
using Core.DomainModel.GDPR;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.GDPR;
using Core.DomainServices.Options;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationDataResponsibleAssigmentServiceTest : WithAutoFixture
    {
        private readonly DataProcessingRegistrationDataResponsibleAssigmentService _sut;
        private readonly Mock<IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption>> _optionsServiceMock;
        private readonly Mock<IGenericRepository<LocalDataProcessingDataResponsibleOption>> _localOptionsRepositoty;


        public DataProcessingRegistrationDataResponsibleAssigmentServiceTest()
        {
            _optionsServiceMock = new Mock<IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption>>();
            _localOptionsRepositoty = new Mock<IGenericRepository<LocalDataProcessingDataResponsibleOption>>();
            _sut = new DataProcessingRegistrationDataResponsibleAssigmentService(
                _optionsServiceMock.Object);
        }

        [Fact]
        public void Can_Assign_DataResponsible()
        {
            //Arrange
            var registration = CreateDpa();
            var origDescription = A<string>();
            var optionId = A<int>();
            var availableOption = new DataProcessingDataResponsibleOption() {
                Id = optionId,
                Description = origDescription
            };
            _optionsServiceMock.Setup(x => x.GetAvailableOption(registration.OrganizationId, optionId)).Returns(availableOption);

            //Act
            var updatedRegistrationResult = _sut.Assign(registration, optionId);

            //Assert
            Assert.True(updatedRegistrationResult.Ok);
            var optionSet = updatedRegistrationResult.Value;
            Assert.Equal(availableOption, optionSet);
        }

        [Fact]
        public void Can_Clear_DataResponsible()
        {
            //Arrange
            var assignedDataResponsible = new DataProcessingDataResponsibleOption();
            var registration = CreateDpa(assignedDataResponsible);
            Assert.Equal(assignedDataResponsible, registration.DataResponsible);

            //Act
            var updatedRegistrationResult = _sut.Clear(registration);

            //Assert
            Assert.True(updatedRegistrationResult.Ok);
            Assert.Equal(assignedDataResponsible, updatedRegistrationResult.Value);
            Assert.Null(registration.DataResponsible);
        }

        [Fact]
        public void Cannot_Clear_DataResponsible_WithNoDataResponsibleAssigned()
        {
            //Arrange
            var registration = CreateDpa();

            //Act
            var updatedRegistrationResult = _sut.Clear(registration);

            //Assert
            Assert.True(updatedRegistrationResult.Failed);
            Assert.Equal(OperationFailure.BadState, updatedRegistrationResult.Error.FailureType);
        }

        [Fact]
        public void Can_Not_Assign_DataResponsible()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Assign(null, A<int>()));
        }


        private DataProcessingRegistration CreateDpa(DataProcessingDataResponsibleOption dataResponsible = null)
        {
            var registration = new DataProcessingRegistration
            {
                OrganizationId = A<int>(),
                DataResponsible = dataResponsible,
            };
            return registration;
        }
    }
}
