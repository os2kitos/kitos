using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.ItSystem;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByTaskRefTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Uuid_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = new ItSystem { TaskRefs = new List<TaskRef>() { new() { Uuid = correctId } } };
            var excludedNoTaskRefs = new ItSystem { TaskRefs = new List<TaskRef>() };
            var excludedWrongUuid = new ItSystem { TaskRefs = new List<TaskRef>() { new() { Uuid = incorrectId } } };

            var input = new[] { excludedWrongUuid, matched, excludedNoTaskRefs }.AsQueryable();
            var sut = new QueryByTaskRef(uuid: correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itSystem = Assert.Single(result);
            Assert.Same(matched, itSystem);
        }

        [Fact]
        public void Apply_Returns_Items_With_Key_Match()
        {
            //Arrange
            var correctId = A<string>();
            var incorrectId = A<string>();
            var matched = new ItSystem { TaskRefs = new List<TaskRef>() { new() { TaskKey = correctId } } };
            var excludedNoTaskRefs = new ItSystem { TaskRefs = new List<TaskRef>() };
            var excludedWrongUuid = new ItSystem { TaskRefs = new List<TaskRef>() { new() { TaskKey = incorrectId } } };

            var input = new[] { excludedWrongUuid, matched, excludedNoTaskRefs }.AsQueryable();
            var sut = new QueryByTaskRef(key: correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itSystem = Assert.Single(result);
            Assert.Same(matched, itSystem);
        }
    }
}
