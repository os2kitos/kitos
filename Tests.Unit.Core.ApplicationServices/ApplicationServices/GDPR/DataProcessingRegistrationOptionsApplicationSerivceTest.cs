using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.GDPR;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.GDPR;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.GDPR
{
    public class DataProcessingRegistrationOptionsApplicationSerivceTest : WithAutoFixture
    {

        private readonly DataProcessingRegistrationOptionsApplicationService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IDataProcessingRegistrationOptionRepository> _optionRepositoryMock;

        public DataProcessingRegistrationOptionsApplicationSerivceTest()
        {
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _optionRepositoryMock = new Mock<IDataProcessingRegistrationOptionRepository>();
            _sut = new DataProcessingRegistrationOptionsApplicationService(
                _authorizationContextMock.Object,
                _optionRepositoryMock.Object);
        }

        [Fact]
        public void Can_GetAssignableOptions()
        {
            //Arrange
            var organizationId = A<int>();
            ExpectOrganizationReadAccess(organizationId);
            var dataResponsibleOptions = new List<OptionDescriptor<DataProcessingDataResponsibleOption>>()
            {
                new OptionDescriptor<DataProcessingDataResponsibleOption>(new DataProcessingDataResponsibleOption(), ""),
            };
            ExpectDataResponsibleOptions(organizationId, dataResponsibleOptions);
            var countryOptions = new List<OptionDescriptor<DataProcessingCountryOption>>()
            {
                new OptionDescriptor<DataProcessingCountryOption>(new DataProcessingCountryOption(), ""),
            };
            ExpectCountryOptions(organizationId, countryOptions);

            //Act
            var assignableOptionsResult = _sut.GetAssignableDataProcessingRegistrationOptions(organizationId);

            //Assert
            Assert.True(assignableOptionsResult.Ok);
            var dataProcessingRegistrationOptions = assignableOptionsResult.Value;
            Assert.Equal(dataResponsibleOptions, dataProcessingRegistrationOptions.DataProcessingRegistrationDataResponsibleOptions);
            Assert.Equal(countryOptions, dataProcessingRegistrationOptions.DataProcessingRegistrationCountryOptions);
        }

        [Fact]
        public void Cannot_GetAssignableOptions_OrganizationAccess()
        {
            //Arrange
            var organizationId = A<int>();

            //Act
            var assignableOptionsResult = _sut.GetAssignableDataProcessingRegistrationOptions(organizationId);

            //Assert
            Assert.True(assignableOptionsResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, assignableOptionsResult.Error.FailureType);
        }

        private void ExpectOrganizationReadAccess(int organizationId)
        {
            _authorizationContextMock.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(OrganizationDataReadAccessLevel.All);
        }

        private void ExpectDataResponsibleOptions(int organizationId, IEnumerable<OptionDescriptor<DataProcessingDataResponsibleOption>> dataResponsibleOptions)
        {
            _optionRepositoryMock.Setup(x => x.GetAvailableDataResponsibleOptions(organizationId)).Returns(dataResponsibleOptions);
        }

        private void ExpectCountryOptions(int organizationId, IEnumerable<OptionDescriptor<DataProcessingCountryOption>> countryOptions)
        {
            _optionRepositoryMock.Setup(x => x.GetAvailableCountryOptions(organizationId)).Returns(countryOptions);
        }

    }
}
