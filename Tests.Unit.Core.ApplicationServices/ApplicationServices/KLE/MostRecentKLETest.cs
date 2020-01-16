using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.KLE;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.KLE
{
    public class MostRecentKLETest
    {
        [Fact]
        private void AddRange_Given_Unique_TasksRefs_Adds_Each_Element()
        {
            var sut = new KLEMostRecent();
            sut.AddRange(new List<TaskRef>
            {
                new TaskRef { TaskKey = "00" },
                new TaskRef { TaskKey = "00.00" },
                new TaskRef { TaskKey = "00.00.00" },
            });
            Assert.Equal(3, sut.GetAll().Count());
        }

        [Fact]
        private void TryGet_Given_Existing_TaskRef_Returns_Element()
        {
            var sut = new KLEMostRecent();
            const string expectedTaskKey = "00.00.00";
            sut.AddRange(new List<TaskRef>
            {
                new TaskRef { TaskKey = expectedTaskKey },
            });
            Assert.True(sut.TryGet(expectedTaskKey, out var resultTaskRef));
            Assert.Equal(expectedTaskKey, resultTaskRef.TaskKey);
        }

        [Fact]
        private void Remove_Given_Existing_TaskRef_Removes_Element()
        {
            var sut = new KLEMostRecent();
            const string expectedTaskKey = "00.00.00";
            sut.AddRange(new List<TaskRef>
            {
                new TaskRef { TaskKey = expectedTaskKey },
            });
            sut.Remove(expectedTaskKey);
            Assert.False(sut.TryGet(expectedTaskKey, out var resultTaskRef));
        }
    }
}
