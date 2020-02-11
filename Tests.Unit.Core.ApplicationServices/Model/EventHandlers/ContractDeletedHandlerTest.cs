using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Context;
using Core.DomainServices.Model.EventHandlers;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model.EventHandlers
{
    public class ContractDeletedHandlerTest : WithAutoFixture
    {
        private readonly ContractDeletedHandler _sut;
        private readonly Mock<IGenericRepository<ItSystemUsage>> _systemUsageRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly ActiveUserContext _activeUserContext;

        public ContractDeletedHandlerTest()
        {
            _systemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _transactionManager = new Mock<ITransactionManager>();
            _activeUserContext = new ActiveUserContext(1337, new User());
            _sut = new ContractDeletedHandler(_systemUsageRepository.Object, _transactionManager.Object, _activeUserContext, Mock.Of<IOperationClock>(x => x.Now == DateTime.Now), Mock.Of<ILogger>());
        }

        [Fact]
        private void Handle_ContractDeleted_Resets_Contract_On_Associated_Relations()
        {
            //Arrange
            var deletedContract = new ItContract();
            deletedContract.AssociatedSystemRelations = new List<SystemRelation>
            {
                CreateRelation(deletedContract),
                CreateRelation(deletedContract),
            };
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            _sut.Handle(new ContractDeleted(deletedContract));

            //Assert that all interface fields were reset
            Assert.True(deletedContract.AssociatedSystemRelations.All(x => x.AssociatedContract == null));
            _systemUsageRepository.Verify(x => x.Save(), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        private SystemRelation CreateRelation(ItContract deletedContract)
        {
            var fromSystemUsage = new ItSystemUsage { Id = A<int>() };
            var systemRelation = new SystemRelation(fromSystemUsage)
            {
                AssociatedContract = deletedContract,
                ToSystemUsage = new ItSystemUsage { Id = A<int>() }
            };
            fromSystemUsage.UsageRelations.Add(systemRelation);
            return systemRelation;
        }
    }
}
