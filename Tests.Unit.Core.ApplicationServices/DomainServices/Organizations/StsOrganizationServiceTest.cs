using System;
using AutoFixture;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;
using Core.DomainServices.Organizations;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.SSO;
using Infrastructure.STS.Common.Model;
using Infrastructure.STS.Organization.DomainServices;
using Kombit.InfrastructureSamples.Token;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.Organizations
{
    public class StsOrganizationServiceTest : WithAutoFixture
    {
        private const string ValidCvr = "12345678";
        private StsOrganizationService _sut;
        private Mock<IStsOrganizationCompanyLookupService> _companyLookupServiceMock;

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            _companyLookupServiceMock = new Mock<IStsOrganizationCompanyLookupService>();
            _sut = new StsOrganizationService(A<StsOrganisationIntegrationConfiguration>(), _companyLookupServiceMock.Object, new Mock<IStsOrganizationIdentityRepository>().Object, 
                new TokenFetcher("", "", "", "", ""), Mock.Of<ILogger>());
        }

        [Theory]
        [InlineData(null)] //not provided
        [InlineData("")] //not provided
        [InlineData(" ")] //not provided
        [InlineData("1234567")] // less than 8
        [InlineData("12345678912")] //more than 10
        public void ValidateConnection_Fails_With_Invalid_Cvr(string cvr)
        {
            //Arrange
            var organization = new Organization { Cvr = cvr };

            //Act
            var error = _sut.ValidateConnection(organization);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(CheckConnectionError.InvalidCvrOnOrganization, error.Value.Detail);
        }

        [Theory]
        [InlineData(OperationFailure.Forbidden, StsError.MissingServiceAgreement, CheckConnectionError.MissingServiceAgreement, OperationFailure.Forbidden)]
        [InlineData(OperationFailure.BadInput, StsError.ExistingServiceAgreementIssue, CheckConnectionError.ExistingServiceAgreementIssue, OperationFailure.BadInput)]
        [InlineData(OperationFailure.BadInput, StsError.NotFound, CheckConnectionError.FailedToLookupOrganizationCompany, OperationFailure.UnknownError)]
        [InlineData(OperationFailure.BadInput, StsError.BadInput, CheckConnectionError.FailedToLookupOrganizationCompany, OperationFailure.UnknownError)]
        public void ValidateConnection_Fails_With_Lookup_Error(OperationFailure failureTypeFromLookup, StsError errorFromLookup, CheckConnectionError expectedError, OperationFailure expectedFailure)
        {
            //Arrange
            var organization = new Organization { Cvr = ValidCvr };
            _companyLookupServiceMock.Setup(x => x.ResolveStsOrganizationCompanyUuid(organization))
                .Returns(new DetailedOperationError<StsError>(failureTypeFromLookup, errorFromLookup));

            //Act
            var error = _sut.ValidateConnection(organization);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(expectedError, error.Value.Detail);
            Assert.Equal(expectedFailure, error.Value.FailureType);
        }

        [Fact]
        public void ValidateConnection_Succeeds_If_Company_Uuuid_Lookup_Succeeds()
        {
            //Arrange
            var organization = new Organization { Cvr = ValidCvr };
            _companyLookupServiceMock.Setup(x => x.ResolveStsOrganizationCompanyUuid(organization)).Returns(A<Guid>());

            //Act
            var error = _sut.ValidateConnection(organization);

            //Assert
            Assert.False(error.HasValue);
        }
    }
}
