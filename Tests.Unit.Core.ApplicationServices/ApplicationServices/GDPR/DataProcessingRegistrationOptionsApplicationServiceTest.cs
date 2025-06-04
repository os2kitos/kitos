using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.GDPR;
using Core.DomainModel.GDPR;
using Core.DomainServices.Authorization;
using Core.DomainServices.Model.Options;
using Core.DomainServices.Repositories.GDPR;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.GDPR
{
    public class DataProcessingRegistrationOptionsApplicationServiceTest : WithAutoFixture
    {

        private readonly DataProcessingRegistrationOptionsApplicationService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IDataProcessingRegistrationOptionRepository> _optionRepositoryMock;

        public DataProcessingRegistrationOptionsApplicationServiceTest()
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
            ExpectOrganizationReadAccess(organizationId, OrganizationDataReadAccessLevel.All);
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
            var basisForTransferOptions = new List<OptionDescriptor<DataProcessingBasisForTransferOption>>()
            {
                new OptionDescriptor<DataProcessingBasisForTransferOption>(new DataProcessingBasisForTransferOption(), ""),
            };
            ExpectBasisForTransferOptions(organizationId, basisForTransferOptions);
            var oversightOptions = new List<OptionDescriptor<DataProcessingOversightOption>>()
            {
                new OptionDescriptor<DataProcessingOversightOption>(new DataProcessingOversightOption(), ""),
            };
            ExpectOversightOptions(organizationId, oversightOptions);

            //Act
            var assignableOptionsResult = _sut.GetAssignableDataProcessingRegistrationOptions(organizationId);

            //Assert
            Assert.True(assignableOptionsResult.Ok);
            var dataProcessingRegistrationOptions = assignableOptionsResult.Value;
            Assert.Equal(dataResponsibleOptions, dataProcessingRegistrationOptions.DataResponsibleOptions);
            Assert.Equal(countryOptions, dataProcessingRegistrationOptions.ThirdCountryOptions);
            Assert.Equal(basisForTransferOptions, dataProcessingRegistrationOptions.BasisForTransferOptions);
            Assert.Equal(oversightOptions, dataProcessingRegistrationOptions.OversightOptions);
        }

        [Theory]
        [InlineData(OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.None)]
        public void Cannot_GetAssignableOptions_OrganizationAccess(OrganizationDataReadAccessLevel readAccessLevel)
        {
            //Arrange
            var organizationId = A<int>();
            ExpectOrganizationReadAccess(organizationId, readAccessLevel);

            //Act
            var assignableOptionsResult = _sut.GetAssignableDataProcessingRegistrationOptions(organizationId);

            //Assert
            Assert.True(assignableOptionsResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, assignableOptionsResult.Error.FailureType);
        }

        private void ExpectOrganizationReadAccess(int organizationId, OrganizationDataReadAccessLevel readAccessLevel)
        {
            _authorizationContextMock.Setup(x => x.GetOrganizationReadAccessLevel(organizationId)).Returns(readAccessLevel);
        }

        private void ExpectDataResponsibleOptions(int organizationId, IEnumerable<OptionDescriptor<DataProcessingDataResponsibleOption>> dataResponsibleOptions)
        {
            _optionRepositoryMock.Setup(x => x.GetAvailableDataResponsibleOptions(organizationId)).Returns(dataResponsibleOptions);
        }

        private void ExpectCountryOptions(int organizationId, IEnumerable<OptionDescriptor<DataProcessingCountryOption>> countryOptions)
        {
            _optionRepositoryMock.Setup(x => x.GetAvailableCountryOptions(organizationId)).Returns(countryOptions);
        }

        private void ExpectBasisForTransferOptions(int organizationId, IEnumerable<OptionDescriptor<DataProcessingBasisForTransferOption>> basisForTransferOptions)
        {
            _optionRepositoryMock.Setup(x => x.GetAvailableBasisForTransferOptions(organizationId)).Returns(basisForTransferOptions);
        }

        private void ExpectOversightOptions(int organizationId, IEnumerable<OptionDescriptor<DataProcessingOversightOption>> oversightOptions)
        {
            _optionRepositoryMock.Setup(x => x.GetAvailableOversightOptions(organizationId)).Returns(oversightOptions);
        }

    }
}
