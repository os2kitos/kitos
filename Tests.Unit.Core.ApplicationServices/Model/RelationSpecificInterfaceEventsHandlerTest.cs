using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
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

namespace Tests.Unit.Core.Model
{
    public class RelationSpecificInterfaceEventsHandlerTest : WithAutoFixture
    {
        private readonly RelationSpecificInterfaceEventsHandler _sut;
        private readonly Mock<IGenericRepository<ItSystemUsage>> _systemUsageRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly ActiveUserContext _activeUserContext;

        public RelationSpecificInterfaceEventsHandlerTest()
        {
            _systemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _transactionManager = new Mock<ITransactionManager>();
            _activeUserContext = new ActiveUserContext(1337, new User());
            _sut = new RelationSpecificInterfaceEventsHandler(_systemUsageRepository.Object, _transactionManager.Object, _activeUserContext, Mock.Of<IOperationClock>(x => x.Now == DateTime.Now), Mock.Of<ILogger>());
        }

        [Fact]
        private void Handle_ExposingSystemChanged_Resets_Interface_On_Associated_Relations()
        {
            //Arrange
            var affectedInterface = new ItInterface();
            affectedInterface.AssociatedSystemRelations = new List<SystemRelation>()
            {
                CreateRelation(affectedInterface),
                CreateRelation(affectedInterface),
            };
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            _sut.Handle(new ExposingSystemChanged(affectedInterface, new ItSystem(), new ItSystem()));

            //Assert that all interface fields were reset
            Assert.True(affectedInterface.AssociatedSystemRelations.All(x => x.RelationInterface == null));
            _systemUsageRepository.Verify(x => x.Save(), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        private void Handle_InterfaceDeleted_Resets_Interface_On_Associated_Relations()
        {
            //Arrange
            var affectedInterface = new ItInterface();
            affectedInterface.AssociatedSystemRelations = new List<SystemRelation>()
            {
                CreateRelation(affectedInterface),
                CreateRelation(affectedInterface),
            };
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            _sut.Handle(new InterfaceDeleted(affectedInterface));

            //Assert that all interface fields were reset
            Assert.True(affectedInterface.AssociatedSystemRelations.All(x => x.RelationInterface == null));
            _systemUsageRepository.Verify(x => x.Save(), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);

        }

        private SystemRelation CreateRelation(ItInterface affectedInterface)
        {
            var fromSystemUsage = new ItSystemUsage { Id = A<int>() };
            var systemRelation = new SystemRelation(fromSystemUsage)
            {
                RelationInterface = affectedInterface,
                ToSystemUsage = new ItSystemUsage { Id = A<int>() }
            };
            fromSystemUsage.UsageRelations.Add(systemRelation);
            return systemRelation;
        }
    }
}
