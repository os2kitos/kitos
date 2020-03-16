using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.BackgroundJobs.Model.ExternalLinks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Qa.References;
using Core.DomainModel.References;
using Core.DomainServices.Repositories.Interface;
using Core.DomainServices.Repositories.Qa;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Time;
using Infrastructure.Services.Configuration;
using Infrastructure.Services.Http;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.BackgroundJobs
{
    public class CheckExternalLinksBackgroundJobTest : WithAutoFixture
    {
        private readonly DateTime _now;
        private readonly Mock<IReferenceRepository> _referenceRepository;
        private readonly Mock<IInterfaceRepository> _interfaceRepository;
        private readonly Mock<IBrokenExternalReferencesReportRepository> _brokenExternalReferencesReportRepository;
        private readonly Mock<IEndpointValidationService> _endpointValidationService;
        private readonly KitosUrl _rootUrl;
        private readonly CheckExternalLinksBackgroundJob _sut;

        public CheckExternalLinksBackgroundJobTest()
        {
            _now = DateTime.Now;
            _referenceRepository = new Mock<IReferenceRepository>();
            _interfaceRepository = new Mock<IInterfaceRepository>();
            _brokenExternalReferencesReportRepository = new Mock<IBrokenExternalReferencesReportRepository>();
            _endpointValidationService = new Mock<IEndpointValidationService>();
            _rootUrl = new KitosUrl(new Uri("https://unit-test.dk"));
            _sut = new CheckExternalLinksBackgroundJob(
                _referenceRepository.Object,
                _interfaceRepository.Object,
                _brokenExternalReferencesReportRepository.Object,
                _endpointValidationService.Object,
                _rootUrl,
                Mock.Of<IOperationClock>(x => x.Now == _now));
        }

        [Fact]
        public async Task ExecuteAsync_Checks_System_And_Interface_External_Links_And_Replaces_Report()
        {
            //Arrange
            var includedInterfaceWhichFails = new ItInterface { Url = $"https://www.test-interface.com/{A<int>()}" };
            var includedInterfaceWhichDoesNotFail = new ItInterface { Url = $"https://www.test-interface.com/{A<int>()}" };
            var excludedInterface = new ItInterface { Url = null };
            var excludedReference = new ExternalReference { URL = $"{_rootUrl.Url.AbsoluteUri}/internal-links-are-ignored" };
            var includedReferenceWhichFails = new ExternalReference { URL = $"https://www.test-system.com/{A<int>()}" };
            var includedReferenceWhichDoesNotFail = new ExternalReference { URL = $"https://www.test-system.com/{A<int>()}" };
            _interfaceRepository.Setup(x => x.GetInterfacesWithExternalReferenceDefined()).Returns(new[] { includedInterfaceWhichFails, includedInterfaceWhichDoesNotFail, excludedInterface }.AsQueryable());
            _referenceRepository.Setup(x => x.GetByRootType(ReferenceRootType.System)).Returns(new[] { excludedReference, includedReferenceWhichFails, includedReferenceWhichDoesNotFail }.AsQueryable());

            var expectedInterfaceError = new EndpointValidationError(EndpointValidationErrorType.CommunicationError);
            var expectedReferenceError = new EndpointValidationError(EndpointValidationErrorType.ErrorResponseCode, HttpStatusCode.NotFound);

            ExpectEndpointValidationResult(includedInterfaceWhichFails.Url, expectedInterfaceError);
            ExpectEndpointValidationResult(includedInterfaceWhichDoesNotFail.Url);
            ExpectEndpointValidationResult(includedReferenceWhichFails.URL, expectedReferenceError);
            ExpectEndpointValidationResult(includedReferenceWhichDoesNotFail.URL);

            //Act
            var result = await _sut.ExecuteAsync();

            //Assert
            Assert.True(result.Ok);
            _brokenExternalReferencesReportRepository.Verify(
                x => x.ReplaceCurrentReport(It.Is<BrokenExternalReferencesReport>(report =>
                    ValidateNewReport(report, includedInterfaceWhichFails, expectedInterfaceError,
                        BrokenLinkCause.CommunicationError, includedReferenceWhichFails, expectedReferenceError,
                        BrokenLinkCause.ErrorResponse))), Times.Once);
        }

        private bool ValidateNewReport(
            BrokenExternalReferencesReport report,
            ItInterface includedInterfaceWhichFails,
            EndpointValidationError expectedInterfaceError,
            BrokenLinkCause brokenLinkCauseForInterface,
            ExternalReference includedReferenceWhichFails,
            EndpointValidationError expectedReferenceError,
            BrokenLinkCause brokenLinkCauseForReference)
        {
            Assert.Equal(_now, report.Created);
            var brokenLinkInInterface = Assert.Single(report.BrokenInterfaceLinks);
            Assert.Same(includedInterfaceWhichFails, brokenLinkInInterface.BrokenReferenceOrigin);
            Assert.Equal(brokenLinkInInterface.ErrorResponseCode, (int?)expectedInterfaceError.StatusCode);
            Assert.Equal(brokenLinkInInterface.Cause, brokenLinkCauseForInterface);
            Assert.Equal(brokenLinkInInterface.ValueOfCheckedUrl, includedInterfaceWhichFails.Url);


            var brokenLinkInExternalReference = Assert.Single(report.BrokenExternalReferences);
            Assert.Same(includedReferenceWhichFails, brokenLinkInExternalReference.BrokenReferenceOrigin);
            Assert.Equal(brokenLinkInExternalReference.ErrorResponseCode, (int?)expectedReferenceError.StatusCode);
            Assert.Equal(brokenLinkInExternalReference.ValueOfCheckedUrl, includedReferenceWhichFails.URL);
            Assert.Equal(brokenLinkInExternalReference.Cause, brokenLinkCauseForReference);

            return true;
        }

        private void ExpectEndpointValidationResult(string url, EndpointValidationError error = null)
        {
            _endpointValidationService.Setup(x => x.ValidateAsync(url)).ReturnsAsync(
                new EndpointValidation(url, error));
        }
    }
}
