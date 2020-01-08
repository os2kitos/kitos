using System;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.KLE;
using NSubstitute;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class KLEApplicationServiceTest
    {
        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, OperationResult.Ok)]
        [InlineData(OrganizationRole.User, OperationResult.Forbidden)]
        private void GetKLEStatus_Authorizes_And_Returns_Valid_KLEStatus(OrganizationRole role, OperationResult expectedOperationResult)
        {
            var mockKLEStandardRepository = Substitute.For<IKLEStandardRepository>();
            var expectedPublishedDate = DateTime.Now.Date;
            var kleStatus = new KLEStatus
            {
                UpToDate = true,
                Published = expectedPublishedDate
            };
            mockKLEStandardRepository.GetKLEStatus().Returns(kleStatus);
            var mockOrganizationalUserContext = Substitute.For<IOrganizationalUserContext>();
            mockOrganizationalUserContext.HasRole(role).Returns(true);
            var sut = new KLEApplicationService(mockOrganizationalUserContext, mockKLEStandardRepository);
            var result = sut.GetKLEStatus();
            Assert.Equal(expectedOperationResult, result.Status);
        }
    }
}
