using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Options;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;

namespace Tests.Unit.Core.ApplicationServices.SystemUsage
{
    public class ItSystemUsageWriteServiceTest : WithAutoFixture
    {
        private readonly Mock<IItSystemUsageService> _itSystemUsageServiceMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IItSystemService> _itSystemServiceMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IOptionsService<ItSystemUsage, ItSystemCategories>> _systemCatagoriesOptionsServiceMock;
        private readonly Mock<IItContractService> _contractServiceMock;
        private readonly Mock<IItProjectService> _projectServiceMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly ItSystemUsageWriteService _sut;

        public ItSystemUsageWriteServiceTest()
        {
            _itSystemUsageServiceMock = new Mock<IItSystemUsageService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _itSystemServiceMock = new Mock<IItSystemService>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _systemCatagoriesOptionsServiceMock = new Mock<IOptionsService<ItSystemUsage, ItSystemCategories>>();
            _contractServiceMock = new Mock<IItContractService>();
            _projectServiceMock = new Mock<IItProjectService>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _sut = new ItSystemUsageWriteService(_itSystemUsageServiceMock.Object, _transactionManagerMock.Object,
                _itSystemServiceMock.Object, _organizationServiceMock.Object, _authorizationContextMock.Object,
                _systemCatagoriesOptionsServiceMock.Object, _contractServiceMock.Object, _projectServiceMock.Object,
                _domainEventsMock.Object, Mock.Of<ILogger>());
        }
    }
}
