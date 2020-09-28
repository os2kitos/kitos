using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.GDPR;
using Core.DomainServices.Options;
using Infrastructure.Services.Types;
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
                _optionsServiceMock.Object,
                _localOptionsRepositoty.Object);
        }

        [Fact]
        public void Can_GetApplicableOptionsWithLocalDescriptionOverrides()
        {
            //Arrange
            var registration = CreateDpa();
            var origDescription = A<string>();
            var availableOptions = new[] { 
                new DataProcessingDataResponsibleOption() { 
                    Description =origDescription,
                } 
            };
            var availableLocalOptions = new[] 
            { 
                new LocalDataProcessingDataResponsibleOption() {
                    OrganizationId = registration.OrganizationId,
                    Description = A<string>(),
                } 
            };
            _optionsServiceMock.Setup(x => x.GetAvailableOptions(registration.OrganizationId)).Returns(availableOptions);
            ExpectLocalOptionsReturns(availableLocalOptions.AsQueryable());

            //Act
            var dataResponsibleOptions = _sut.GetApplicableDataResponsibleOptionsWithLocalDescriptionOverrides(registration);

            //Assert
            var dataResponsibleOption = Assert.Single(dataResponsibleOptions);
            Assert.Equal(availableOptions[0].Id, dataResponsibleOption.Id);
            Assert.Equal(availableLocalOptions[0].Description, dataResponsibleOption.Description);
        }

        [Fact]
        public void Can_GetApplicableOptionsWithNoLocalDescriptionOverrides()
        {
            //Arrange
            var registration = CreateDpa();
            var origDescription = A<string>();
            var availableOptions = new[] {
                new DataProcessingDataResponsibleOption() {
                    Description =origDescription,
                }
            };
            var availableLocalOptions = new List<LocalDataProcessingDataResponsibleOption>();
            _optionsServiceMock.Setup(x => x.GetAvailableOptions(registration.OrganizationId)).Returns(availableOptions);
            ExpectLocalOptionsReturns(availableLocalOptions.AsQueryable());

            //Act
            var dataResponsibleOptions = _sut.GetApplicableDataResponsibleOptionsWithLocalDescriptionOverrides(registration);

            //Assert
            var dataResponsibleOption = Assert.Single(dataResponsibleOptions);
            Assert.Equal(availableOptions[0].Id, dataResponsibleOption.Id);
            Assert.Equal(origDescription, dataResponsibleOption.Description);
        }

        [Fact]
        public void Can_Not_GetApplicableOptionsWithNoLocalDescriptionOverrides_If_No_Registration()
        {
            //Arrange
            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => _sut.GetApplicableDataResponsibleOptionsWithLocalDescriptionOverrides(null));
        }

        [Fact]
        public void Can_Update_DataResponsible()
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
            var updatedRegistrationResult = _sut.UpdateDataResponsible(registration, optionId);

            //Assert
            Assert.True(updatedRegistrationResult.Ok);
            var updatedRegistration = updatedRegistrationResult.Value;
            Assert.Equal(availableOption, updatedRegistration.DataResponsible);
        }

        [Fact]
        public void Can_Update_DataResponsible_To_Null()
        {
            //Arrange
            var registration = CreateDpa();
            var origDescription = A<string>();
            var optionId = A<int>();

            //Act
            var updatedRegistrationResult = _sut.UpdateDataResponsible(registration, null);

            //Assert
            Assert.True(updatedRegistrationResult.Ok);
            var updatedRegistration = updatedRegistrationResult.Value;
            Assert.Null(updatedRegistration.DataResponsible);
        }

        [Fact]
        public void Can_Not_Update_DataResponsible()
        {
            //Arrange
            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => _sut.UpdateDataResponsible(null, null));
        }


        private DataProcessingRegistration CreateDpa((int righRole, int rightUserId)? right = null)
        {
            var registration = new DataProcessingRegistration
            {
                OrganizationId = A<int>(),
            };
            return registration;
        }

        private void ExpectLocalOptionsReturns(IQueryable<LocalDataProcessingDataResponsibleOption> result)
        {
            _localOptionsRepositoty.Setup(x => x
                    .AsQueryable())
                .Returns(result);
        }
    }
}
