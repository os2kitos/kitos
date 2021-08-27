using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.GDPR.Write;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Moq;
using Serilog;

namespace Tests.Unit.Core.ApplicationServices.GDPR
{
    public class DataProcessingRegistrationWriteServiceTest
    {
        private readonly DataProcessingRegistrationWriteService _sut;
        private readonly Mock<IDataProcessingRegistrationApplicationService> _dprServiceMock;
        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IDatabaseControl> _databaseControlMock;

        public DataProcessingRegistrationWriteServiceTest()
        {
            _dprServiceMock = new Mock<IDataProcessingRegistrationApplicationService>();
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _sut = new DataProcessingRegistrationWriteService(
                _dprServiceMock.Object,
                _identityResolverMock.Object,
                Mock.Of<ILogger>(),
                _domainEventsMock.Object,
                _transactionManagerMock.Object,
                _databaseControlMock.Object);
        }
    }
}
