using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Extensions;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Extensions
{
    public class QueryableApiResponseOrderingExtensionsTest : WithAutoFixture
    {
        public class GenericEntityClass : Entity, IHasName
        {
            public string Name { get; set; }
        }

        private int _nextId;

        public QueryableApiResponseOrderingExtensionsTest()
        {
            _nextId = 0;
        }

        [Fact]
        public void OrderApiResultsByDefaultConventions_OrdersFirstByLastChanged_If_Source_Is_PreFiltered()
        {
            //Arrange
            var now = A<DateTime>();
            var last = CreateGenericEntityClass(lastChanged: now.AddDays(1));
            var first = CreateGenericEntityClass(lastChanged:now,name:"aa"); //should come before "middle" due to thenBy on name
            var middle = CreateGenericEntityClass(lastChanged:now,name:"ab");

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderApiResultsByDefaultConventions(true,CommonOrderByProperty.Name);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Fact]
        public void OrderApiResultsByDefaultConventions_Only_Uses_AdditionalFilter_If_Source_Is_Not_PreFiltered()
        {
            //Arrange
            var now = A<DateTime>();
            var last = CreateGenericEntityClass(lastChanged:now.AddDays(-1),name:"ac");
            var first = CreateGenericEntityClass(lastChanged: now, name: "aa");
            var middle = CreateGenericEntityClass(lastChanged: now.AddDays(100), name: "ab");

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderApiResultsByDefaultConventions(false, CommonOrderByProperty.Name);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OrderApiResults_By_CreationOrder(bool passOrderingParam)
        {
            //Arrange

            //Verify that it falls back to creation order (db id) if not provided
            CommonOrderByProperty? orderByProperty = passOrderingParam ? CommonOrderByProperty.CreationOrder : null;
            var last = CreateGenericEntityClass(9);
            var first = CreateGenericEntityClass(1);
            var middle = CreateGenericEntityClass(2);

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderApiResults(orderByProperty);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Fact]
        public void OrderApiResults_By_Name()
        {
            //Arrange
            var last = CreateGenericEntityClass(name: "09_last");
            var first = CreateGenericEntityClass(name: "07_first");
            var middle = CreateGenericEntityClass(name: "08_middle");

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderApiResults(CommonOrderByProperty.Name);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Fact]
        public void OrderApiResults_By_LastChanged()
        {
            //Arrange
            var now = DateTime.Now;
            var last = CreateGenericEntityClass(lastChanged: now.AddTicks(1));
            var first = CreateGenericEntityClass(lastChanged: now.AddTicks(-1));
            var middle = CreateGenericEntityClass(lastChanged: now);

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderApiResults(CommonOrderByProperty.LastChanged);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OrderUserApiResults_By_CreationOrder(bool passOrderingParam)
        {
            //Arrange

            //Verify that it falls back to creation order (db id) if not provided
            CommonOrderByProperty? orderByProperty = passOrderingParam ? CommonOrderByProperty.CreationOrder : null;
            var last = CreateUser(9);
            var first = CreateUser(1);
            var middle = CreateUser(2);

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderUserApiResults(orderByProperty);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Fact]
        public void OrderUserApiResults_By_Name()
        {
            //Arrange
            var last = CreateUser(name: "09_last");
            var first = CreateUser(name: "07_first", lastName: "aa");
            var middle = CreateUser(name: "07_first", lastName: "ab");

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderUserApiResults(CommonOrderByProperty.Name);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Fact]
        public void OrderUserApiResults_By_LastChanged()
        {
            //Arrange
            var now = DateTime.Now;
            var last = CreateUser(lastChanged: now.AddTicks(1));
            var first = CreateUser(lastChanged: now.AddTicks(-1));
            var middle = CreateUser(lastChanged: now);

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderUserApiResults(CommonOrderByProperty.LastChanged);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OrderSystemUsageApiResults_By_CreationOrder(bool passOrderingParam)
        {
            //Arrange

            //Verify that it falls back to creation order (db id) if not provided
            CommonOrderByProperty? orderByProperty = passOrderingParam ? CommonOrderByProperty.CreationOrder : null;
            var last = CreateSystemUsage(9);
            var first = CreateSystemUsage(1);
            var middle = CreateSystemUsage(2);

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderSystemUsageApiResults(orderByProperty);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Fact]
        public void OrderSystemUsageApiResults_By_Name()
        {
            //Arrange
            var last = CreateSystemUsage(name: "09_last");
            var first = CreateSystemUsage(name: "07_first", lastName: "aa");
            var middle = CreateSystemUsage(name: "07_first", lastName: "ab");

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderSystemUsageApiResults(CommonOrderByProperty.Name);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Fact]
        public void OrderSystemUsageApiResults_By_LastChanged()
        {
            //Arrange
            var now = DateTime.Now;
            var last = CreateSystemUsage(lastChanged: now.AddTicks(1));
            var first = CreateSystemUsage(lastChanged: now.AddTicks(-1));
            var middle = CreateSystemUsage(lastChanged: now);

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderSystemUsageApiResults(CommonOrderByProperty.LastChanged);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Fact]
        public void OrderSystemUsageApiResultsByDefaultConventions_OrdersFirstByLastChanged_If_Source_Is_PreFiltered()
        {
            //Arrange
            var now = A<DateTime>();
            var last = CreateSystemUsage(lastChanged: now.AddDays(1));
            var first = CreateSystemUsage(lastChanged: now, name: "aa"); //should come before "middle" due to thenBy on name
            var middle = CreateSystemUsage(lastChanged: now, name: "ab");

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderSystemUsageApiResultsByDefaultConventions(true, CommonOrderByProperty.Name);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        [Fact]
        public void OrderSystemUsageApiResultsByDefaultConventions_Only_Uses_AdditionalFilter_If_Source_Is_Not_PreFiltered()
        {
            //Arrange
            var now = A<DateTime>();
            var last = CreateSystemUsage(lastChanged: now.AddDays(-1), name: "ac");
            var first = CreateSystemUsage(lastChanged: now, name: "aa");
            var middle = CreateSystemUsage(lastChanged: now.AddDays(100), name: "ab");

            var input = new[] { last, first, middle }.AsQueryable();

            //Act
            var orderedResult = input.OrderSystemUsageApiResultsByDefaultConventions(false, CommonOrderByProperty.Name);

            //Assert
            var expected = new[] { first, middle, last };
            VerifyOrdering(expected, orderedResult);
        }

        private static void VerifyOrdering<T>(IEnumerable<T> expected, IQueryable<T> orderedResult) where T : IHasId
        {
            Assert.Equal(expected.Select(x => x.Id).ToList(), orderedResult.Select(x => x.Id).ToList());
        }

        private GenericEntityClass CreateGenericEntityClass(int? id = null, DateTime? lastChanged = null, string name = null)
        {
            var entity = new GenericEntityClass();
            AssignCommonFields(id, lastChanged, entity);
            entity.Name = name ?? A<string>();
            return entity;
        }

        private User CreateUser(int? id = null, DateTime? lastChanged = null, string name = null, string lastName = null)
        {
            var entity = new User();
            AssignCommonFields(id, lastChanged, entity);
            entity.Name = name ?? A<string>();
            entity.LastName = lastName ?? A<string>();
            return entity;
        }

        private ItSystemUsage CreateSystemUsage(int? id = null, DateTime? lastChanged = null, string name = null, string lastName = null)
        {
            var entity = new ItSystemUsage() { ItSystem = new ItSystem() };
            AssignCommonFields(id, lastChanged, entity);
            entity.ItSystem.Name = name ?? A<string>();
            return entity;
        }

        private void AssignCommonFields(int? id, DateTime? lastChanged, IEntity entity)
        {
            entity.Id = id ?? _nextId++;
            entity.LastChanged = lastChanged ?? A<DateTime>();
        }
    }
}
