using System;
using System.Collections.Generic;
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
        private readonly Mock<IGenericRepository<KLEUpdateHistoryItem>> _mockGenericUpdateHistoryItemRepository;
        private readonly Mock<ITransactionManager> _mockTransactionManager;
        private readonly KLEUpdateHistoryItemRepository _sut;

        public KLEUpdateHistoryItemRepositoryTest()
        {
            _mockGenericUpdateHistoryItemRepository = new Mock<IGenericRepository<KLEUpdateHistoryItem>>();
            _mockTransactionManager = new Mock<ITransactionManager>();
            _sut = new KLEUpdateHistoryItemRepository(_mockGenericUpdateHistoryItemRepository.Object, _mockTransactionManager.Object);

        }

        [Fact]
        public void Get_Returns_All_UpdateHistoryItems()
        {
            //Arrange
            _mockGenericUpdateHistoryItemRepository.Setup(
                r => r.Get(
                    It.IsAny<Expression<Func<KLEUpdateHistoryItem, bool>>>(), 
                    It.IsAny<Func<IQueryable<KLEUpdateHistoryItem>, IOrderedQueryable<KLEUpdateHistoryItem>>>(),
                    It.IsAny<string>())).Returns(new List<KLEUpdateHistoryItem>
            {
                new(DateTime.Parse("01-01-2020", CultureInfo.GetCultureInfo("da-DK"))),
                new(DateTime.Parse("01-11-2019", CultureInfo.GetCultureInfo("da-DK")))
            });

            //Act
            var result = _sut.Get();

            //Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetLastest_Returns_LastChanged_UpdateHistoryItem()
        {
            //Arrange
            var expectedLastUpdate = DateTime.Today;
            _mockGenericUpdateHistoryItemRepository.Setup(r => r.Count).Returns(1);
            _mockGenericUpdateHistoryItemRepository
                .Setup(r => r.GetMax(It.IsAny<Expression<Func<KLEUpdateHistoryItem, DateTime>>>()))
                .Returns(expectedLastUpdate);

            //Act
            var result = _sut.GetLastUpdated();

            //Assert
            Assert.Equal(expectedLastUpdate, result.Value);
        }

        [Fact]
        public void Insert_Updates_And_Remembers()
        {
            //Arrange
            _mockGenericUpdateHistoryItemRepository.Setup(r => r.Insert(It.IsAny<KLEUpdateHistoryItem>()));
            var mockDatabaseTransaction = new Mock<IDatabaseTransaction>();
            _mockTransactionManager.Setup(t => t.Begin())
                .Returns(mockDatabaseTransaction.Object);

            //Act
            _sut.Insert(DateTime.Parse("01-11-2019", CultureInfo.GetCultureInfo("da-DK")));

            //Assert
            _mockGenericUpdateHistoryItemRepository.VerifyAll();
        }
    }
}
