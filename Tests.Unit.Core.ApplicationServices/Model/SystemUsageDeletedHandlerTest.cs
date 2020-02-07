using System;
using System.Collections.Generic;
using System.Data;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.DomainEvents;
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
    public class SystemUsageDeletedHandlerTest : WithAutoFixture
    {
        private readonly SystemUsageDeletedHandler _sut;
        private readonly Mock<IGenericRepository<ItSystemUsage>> _systemUsageRepository;
        private readonly Mock<IGenericRepository<SystemRelation>> _systemRelationRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly ActiveUserContext _activeUserContext;

        public SystemUsageDeletedHandlerTest()
        {
            _systemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _systemRelationRepository = new Mock<IGenericRepository<SystemRelation>>();
            _transactionManager = new Mock<ITransactionManager>();
            _activeUserContext = new ActiveUserContext(1337, new User());
            _sut = new SystemUsageDeletedHandler(
                _systemUsageRepository.Object,
                _systemRelationRepository.Object,
                _transactionManager.Object,
                Mock.Of<IOperationClock>(x => x.Now == DateTime.Now),
                Mock.Of<ILogger>(),
                _activeUserContext);
        }

        [Fact]
        public void Handle_Deletes_All_UsedBy_Relations_From_Owner_Usages()
        {
            //Arrange
            var deletedSystemUsage = new ItSystemUsage { Id = A<int>() };
            var relation1 = CreateRelation(deletedSystemUsage);
            var relation2 = CreateRelation(deletedSystemUsage);
            deletedSystemUsage.UsedByRelations.Add(relation1);
            deletedSystemUsage.UsedByRelations.Add(relation2);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

            //Act
            _sut.Handle(new SystemUsageDeleted(deletedSystemUsage));

            //Assert that model was updated and that deleted relations were marked in repository
            Assert.False(relation1.FromSystemUsage.UsageRelations.Contains(relation1));
            Assert.False(relation2.FromSystemUsage.UsageRelations.Contains(relation2));
            Assert.Same(_activeUserContext.UserEntity, relation1.FromSystemUsage.LastChangedByUser);
            Assert.Same(_activeUserContext.UserEntity, relation2.FromSystemUsage.LastChangedByUser);
            _systemRelationRepository.Verify(x => x.Delete(It.IsAny<SystemRelation>()), Times.Exactly(2));
            _systemRelationRepository.Verify(x => x.Delete(relation1), Times.Once);
            _systemRelationRepository.Verify(x => x.Delete(relation2), Times.Once);
            _systemRelationRepository.Verify(x => x.Save(), Times.Once);
            _systemUsageRepository.Verify(x => x.Save(), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        private SystemRelation CreateRelation(ItSystemUsage deletedSystemUsage)
        {
            var fromSystemUsage = new ItSystemUsage { Id = A<int>() };
            var systemRelation = new SystemRelation(fromSystemUsage)
            {
                FromSystemUsageId = fromSystemUsage.Id,
                ToSystemUsage = deletedSystemUsage,
                Id = A<int>()
            };
            fromSystemUsage.UsageRelations.Add(systemRelation);
            return systemRelation;
        }
    }
}
