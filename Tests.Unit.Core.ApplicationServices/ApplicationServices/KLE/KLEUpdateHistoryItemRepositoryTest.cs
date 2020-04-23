using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
        private readonly Mock<IGenericRepository<KLEUpdateHistoryItem>> mockGenericUpdateHistoryItemRepository;
        private readonly Mock<ITransactionManager> mockTransactionManager;
        private readonly KLEUpdateHistoryItemRepository sut;

        public KLEUpdateHistoryItemRepositoryTest()
        {
            mockGenericUpdateHistoryItemRepository = new Mock<IGenericRepository<KLEUpdateHistoryItem>>();
            mockTransactionManager = new Mock<ITransactionManager>();
            sut = new KLEUpdateHistoryItemRepository(mockGenericUpdateHistoryItemRepository.Object, mockTransactionManager.Object);

        }

        [Fact]
        private void Get_Returns_All_UpdateHistoryItems()
        {
            //Arrange
            mockGenericUpdateHistoryItemRepository.Setup(
                r => r.Get(
                    It.IsAny<Expression<Func<KLEUpdateHistoryItem, bool>>>(), 
                    It.IsAny<Func<IQueryable<KLEUpdateHistoryItem>, IOrderedQueryable<KLEUpdateHistoryItem>>>(),
                    It.IsAny<string>())).Returns(new List<KLEUpdateHistoryItem>
            {
                new KLEUpdateHistoryItem(DateTime.Parse("01-01-2020", CultureInfo.GetCultureInfo("da-DK"))),
                new KLEUpdateHistoryItem(DateTime.Parse("01-11-2019", CultureInfo.GetCultureInfo("da-DK")))
            });

            //Act
            var result = sut.Get();

            //Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        private void GetLastest_Returns_LastChanged_UpdateHistoryItem()
        {
            //Arrange
            var expectedLastUpdate = DateTime.Today;
            mockGenericUpdateHistoryItemRepository.Setup(r => r.Count).Returns(1);
            mockGenericUpdateHistoryItemRepository
                .Setup(r => r.GetMax(It.IsAny<Expression<Func<KLEUpdateHistoryItem, DateTime>>>()))
                .Returns(expectedLastUpdate);

            //Act
            var result = sut.GetLastUpdated();

            //Assert
            Assert.Equal(expectedLastUpdate, result);
        }

        [Fact]
        private void Insert_Updates_And_Remembers()
        {
            //Arrange
            mockGenericUpdateHistoryItemRepository.Setup(r => r.Insert(It.IsAny<KLEUpdateHistoryItem>()));
            var mockDatabaseTransaction = new Mock<IDatabaseTransaction>();
            mockTransactionManager.Setup(t => t.Begin(IsolationLevel.Serializable))
                .Returns(mockDatabaseTransaction.Object);

            //Act
            sut.Insert(DateTime.Parse("01-11-2019", CultureInfo.GetCultureInfo("da-DK")));

            //Assert
            mockGenericUpdateHistoryItemRepository.VerifyAll();
        }
    }
}
