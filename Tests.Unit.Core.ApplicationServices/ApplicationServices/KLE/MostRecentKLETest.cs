using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.KLE
{
    public class MostRecentKLETest
    {
        private readonly KLEMostRecent _sut;

        public MostRecentKLETest()
        {
            _sut = new KLEMostRecent();
        }

        [Fact]
        private void AddRange_Given_Unique_TasksRefs_Adds_Each_Element()
        {
            _sut.AddRange(new List<TaskRef>
            {
                new TaskRef { TaskKey = "00" },
                new TaskRef { TaskKey = "00.00" },
                new TaskRef { TaskKey = "00.00.00" },
            });

            Assert.Equal(3, _sut.GetAll().Count());
        }

        [Fact]
        private void TryGet_Given_Existing_TaskRef_Returns_Element()
        {
            //Arrange
            const string expectedTaskKey = "00.00.00";
            _sut.AddRange(new List<TaskRef>
            {
                new TaskRef { TaskKey = expectedTaskKey },
            });

            //Act
            var result = _sut.TryGet(expectedTaskKey, out var resultTaskRef);

            //Assert
            Assert.True(result);
            Assert.Equal(expectedTaskKey, resultTaskRef.TaskKey);
        }

        [Fact]
        private void Remove_Given_Existing_TaskRef_Removes_Element()
        {
            //Arrange
            const string expectedTaskKey = "00.00.00";
            _sut.AddRange(new List<TaskRef>
            {
                new TaskRef { TaskKey = expectedTaskKey },
            });

            //Act
            _sut.Remove(expectedTaskKey);

            //Assert
            Assert.False(_sut.TryGet(expectedTaskKey, out _));
        }
    }
}
