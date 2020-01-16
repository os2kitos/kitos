using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Core.DomainModel.KLE;
using Core.DomainServices;
using Core.DomainServices.Repositories.KLE;
using Infrastructure.Services.DataAccess;
using Moq;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.KLE
{
    public class KLEUpdateHistoryItemRepositoryTest
    {
        [Fact]
        private void Get_Returns_All_UpdateHistoryItems()
        {
            var mockGenericUpdateHistoryItemRepository = new Mock<IGenericRepository<KLEUpdateHistoryItem>>();
            mockGenericUpdateHistoryItemRepository.Setup(
                r => r.Get(
                    It.IsAny<Expression<Func<KLEUpdateHistoryItem, bool>>>(), 
                    It.IsAny<Func<IQueryable<KLEUpdateHistoryItem>, IOrderedQueryable<KLEUpdateHistoryItem>>>(),
                    It.IsAny<string>())).Returns(new List<KLEUpdateHistoryItem>
            {
                new KLEUpdateHistoryItem("01-01-2020", 1),
                new KLEUpdateHistoryItem("01-11-2019", 1)
            });
            var mockTransactionManager = new Mock<ITransactionManager>();
            var sut = new KLEUpdateHistoryItemRepository(mockGenericUpdateHistoryItemRepository.Object, mockTransactionManager.Object);
            var result = sut.Get();
            Assert.Equal(2, result.Count());
        }

        [Fact]
        private void GetLastest_Returns_LastChanged_UpdateHistoryItem()
        {
            var mockGenericUpdateHistoryItemRepository = new Mock<IGenericRepository<KLEUpdateHistoryItem>>();
            var expectedLastUpdate = DateTime.Today;
            mockGenericUpdateHistoryItemRepository.Setup(r => r.Count).Returns(1);
            mockGenericUpdateHistoryItemRepository
                .Setup(r => r.GetMax(It.IsAny<Expression<Func<KLEUpdateHistoryItem, DateTime>>>()))
                .Returns(expectedLastUpdate);
            var mockTransactionManager = new Mock<ITransactionManager>();
            var sut = new KLEUpdateHistoryItemRepository(mockGenericUpdateHistoryItemRepository.Object, mockTransactionManager.Object);
            var result = sut.GetLastUpdated();
            Assert.Equal(expectedLastUpdate, result);
        }

        [Fact]
        private void Insert_Updates_And_Remembers()
        {
            var mockGenericUpdateHistoryItemRepository = new Mock<IGenericRepository<KLEUpdateHistoryItem>>();
            mockGenericUpdateHistoryItemRepository.Setup(r => r.Insert(It.IsAny<KLEUpdateHistoryItem>()));
            var mockTransactionManager = new Mock<ITransactionManager>();
            var mockDatabaseTransaction = new Mock<IDatabaseTransaction>();
            mockTransactionManager.Setup(t => t.Begin(IsolationLevel.Serializable))
                .Returns(mockDatabaseTransaction.Object);
            var sut = new KLEUpdateHistoryItemRepository(mockGenericUpdateHistoryItemRepository.Object, mockTransactionManager.Object);
            sut.Insert("01-11-2019", 1);
            mockGenericUpdateHistoryItemRepository.Verify();
        }
    }
}
