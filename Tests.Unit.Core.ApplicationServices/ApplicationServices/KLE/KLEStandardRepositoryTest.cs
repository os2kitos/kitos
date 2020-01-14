using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Repositories.KLE;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.KLEDataBridge;
using NSubstitute;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.KLE
{
    public class KLEStandardRepositoryTest
    {
        [Theory]
        [InlineData("2019-11-01", true)]
        [InlineData("9999-12-31", false)]
        private void GetKLEStatus_Returns_ValidStatus(string currentDate, bool expectedUpToDate)
        {
            var mockKLEDataBridge = Substitute.For<IKLEDataBridge>();
            var document = XDocument.Load("./ApplicationServices/KLE/20200106-kle-only-published-date.xml");
            var publishedDate = DateTime.Parse(currentDate);
            document.Descendants("UdgivelsesDato").First().Value = publishedDate.ToLongDateString();
            mockKLEDataBridge.GetKLEXMLData().Returns(document);
            var mockTaskRefRepository = Substitute.For<IGenericRepository<TaskRef>>();
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, mockTaskRefRepository, null, null);
            var result = sut.GetKLEStatus();
            Assert.Equal(expectedUpToDate, result.UpToDate);
            Assert.Equal(publishedDate, result.Published);
        }

        [Fact]
        private void GetKLEChangeSummary_Returns_List_Of_Changes()
        {
            var mockKLEDataBridge = Substitute.For<IKLEDataBridge>();
            var document = XDocument.Load("./ApplicationServices/KLE/20200106-kle-sample-changes.xml");
            mockKLEDataBridge.GetKLEXMLData().Returns(document);
            var mockTaskRefRepository = Substitute.For<IGenericRepository<TaskRef>>();
            var taskRefs = new List<TaskRef>
            {
                // Removed item examples
                SetupTaskRef("KLE-Hovedgruppe", "03"),
                SetupTaskRef("KLE-Gruppe", "00.02" ),
                SetupTaskRef("KLE-Emne", "00.03.01"),
                // Renamed item example
                SetupTaskRef("KLE-Emne", "00.03.00", "International virksomhed og EU"),
                // Unchanged item example
                SetupTaskRef("KLE-Emne", "02.02.00", "Bebyggelsens højde- og afstandsforhold i almindelighed"),
            };
            mockTaskRefRepository.Get().Returns(taskRefs);
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, mockTaskRefRepository, null, null);
            var result = sut.GetKLEChangeSummary();
            var numberOfKLEMainGroups = document.Descendants("Hovedgruppe").Count();
            var numberOfKLEGroups = document.Descendants("Gruppe").Count();
            var numberOfKLESubjects = document.Descendants("Emne").Count();
            var totalKLEItems = numberOfKLEMainGroups + numberOfKLEGroups + numberOfKLESubjects;
            const int expectedNumberOfRemoved = 3;
            const int expectedNumberOfRenames = 1;
            const int expectedNumberOfUnchanged = 1;
            var expectedNumberOfAdded = totalKLEItems - expectedNumberOfRenames - expectedNumberOfUnchanged;
            Assert.Equal(3, numberOfKLEMainGroups);
            Assert.Equal(9, numberOfKLEGroups);
            Assert.Equal(9, numberOfKLESubjects);
            Assert.Equal(expectedNumberOfRemoved, result.Count(c => c.ChangeType == KLEChangeType.Removed));
            Assert.Equal(expectedNumberOfRenames, result.Count(c => c.ChangeType == KLEChangeType.Renamed));
            Assert.Equal(expectedNumberOfAdded, result.Count(c => c.ChangeType == KLEChangeType.Added));
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_Renamed_TaskRef()
        {
            var mockKLEDataBridge = SetupUpdateObjects(out var mockTaskRefRepository, out var renamedTaskRef,
                out var removedTaskRef);
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockItProjectRepository = Substitute.For<IGenericRepository<ItProject>>();
            var mockItSystemRepository = Substitute.For<IGenericRepository<ItSystem>>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, mockTaskRefRepository, mockItProjectRepository, mockItSystemRepository);
            sut.UpdateKLE();
            Assert.Equal("HAS BEEN RENAMED", renamedTaskRef.Description);
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_Both_TaskRef_And_ItProject()
        {
            var mockKLEDataBridge = SetupUpdateObjects(out var mockTaskRefRepository, out var renamedTaskRef, out var removedTaskRef);
            var mockItProjectRepository = Substitute.For<IGenericRepository<ItProject>>();
            const int itProjectKey = 1;
            var itProject = new ItProject
            {
                Id = itProjectKey,
                TaskRefs = new List<TaskRef> {removedTaskRef}
            };
            removedTaskRef.ItProjects = new List<ItProject> { itProject };
            mockItProjectRepository.GetByKey(itProjectKey).Returns(itProject);
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockItSystemRepository = Substitute.For<IGenericRepository<ItSystem>>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, mockTaskRefRepository, mockItProjectRepository, mockItSystemRepository);
            sut.UpdateKLE();
            Assert.False(itProject.TaskRefs.Contains(removedTaskRef));
            Assert.Null(removedTaskRef.ItProjects.FirstOrDefault(p => p.Id == itProjectKey));
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_Both_TaskRef_And_ItSystem()
        {
            var mockKLEDataBridge = SetupUpdateObjects(out var mockTaskRefRepository, out var renamedTaskRef,
                out var removedTaskRef);
            var mockItSystemRepository = Substitute.For<IGenericRepository<ItSystem>>();
            const int itSystemKey = 1;
            var itSystem = new ItSystem
            {
                Id = itSystemKey,
                TaskRefs = new List<TaskRef> { removedTaskRef }
            };
            removedTaskRef.ItSystems = new List<ItSystem> { itSystem };
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockItProjectRepository = Substitute.For<IGenericRepository<ItProject>>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, mockTaskRefRepository, mockItProjectRepository, mockItSystemRepository);
            sut.UpdateKLE();
            Assert.False(itSystem.TaskRefs.Contains(removedTaskRef));
            Assert.Null(removedTaskRef.ItProjects.FirstOrDefault(p => p.Id == itSystemKey));
        }

        #region Helpers

        private static IKLEDataBridge SetupUpdateObjects(out IGenericRepository<TaskRef> mockTaskRefRepository,
            out TaskRef renamedTaskRef, out TaskRef removedTaskRef)
        {
            var mockKLEDataBridge = Substitute.For<IKLEDataBridge>();
            var document = XDocument.Load("./ApplicationServices/KLE/20200106-kle-sample-changes.xml");
            mockKLEDataBridge.GetKLEXMLData().Returns(document);
            mockTaskRefRepository = Substitute.For<IGenericRepository<TaskRef>>();
            renamedTaskRef = SetupTaskRef("KLE-Emne", "00.03.00", "International virksomhed og EU");
            removedTaskRef = SetupTaskRef("KLE-Emne", "00.03.01", "Dummy");
            var existingTaskRefs = new List<TaskRef> {renamedTaskRef, removedTaskRef};
            mockTaskRefRepository.Get().Returns(existingTaskRefs);
            mockTaskRefRepository.GetWithReferencePreload(SetupTaskRefFilter(renamedTaskRef)).Returns(new List<TaskRef> {renamedTaskRef});
            mockTaskRefRepository.GetWithReferencePreload(SetupTaskRefFilter(removedTaskRef)).Returns(new List<TaskRef> {removedTaskRef});
            return mockKLEDataBridge;
        }

        private static Expression<Func<TaskRef, bool>> SetupTaskRefFilter(TaskRef renamedTaskRef)
        {
            return Arg.Is<Expression<Func<TaskRef, bool>>>(
                e => Neleus.LambdaCompare.Lambda.Eq(e, t => t.TaskKey == renamedTaskRef.TaskKey)
            );
        }

        private static TaskRef SetupTaskRef(string kleType, string kleTaskKey, string kleDescription = "")
        {
            return new TaskRef { Type = kleType, TaskKey = kleTaskKey, Description = kleDescription };
        }

        #endregion
    }
}
