using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Repositories.KLE;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.KLEDataBridge;
using NSubstitute;
using Serilog;
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
            var stubTaskRefRepository = new GenericRepositoryTaskRefStub();
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockLogger = Substitute.For<ILogger>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, stubTaskRefRepository, mockLogger);
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
            var stubTaskRefRepository = new GenericRepositoryTaskRefStub();
            // Removed item examples
            stubTaskRefRepository.Insert(SetupTaskRef(1, "KLE-Hovedgruppe", "03"));
            stubTaskRefRepository.Insert(SetupTaskRef(2, "KLE-Gruppe", "00.02" ));
            stubTaskRefRepository.Insert(SetupTaskRef(3, "KLE-Emne", "00.03.01"));
            // Renamed item example
            stubTaskRefRepository.Insert(SetupTaskRef(4, "KLE-Emne", "00.03.00", "International virksomhed og EU"));
            // Unchanged item example
            stubTaskRefRepository.Insert(SetupTaskRef(5, "KLE-Emne", "02.02.00", "Bebyggelsens højde- og afstandsforhold i almindelighed"));
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockLogger = Substitute.For<ILogger>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, stubTaskRefRepository, mockLogger);
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
        private void UpdateKLE_Given_Summary_Updates_Renamed_TaskRefs()
        {
            var mockKLEDataBridge = SetupUpdateObjects(out var stubTaskRefRepository, out var removedTaskRef, out var renamedTaskRef);
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockLogger = Substitute.For<ILogger>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, stubTaskRefRepository, mockLogger);
            sut.UpdateKLE(0, 0);
            Assert.Equal("HAS BEEN RENAMED", renamedTaskRef.Description);
            Assert.Equal(Guid.Parse("f8d6e719-e050-48d8-89e2-977d0eaba6bb"), renamedTaskRef.Uuid);
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_On_Empty_Repos_Adds_All_TaskRefs()
        {
            var mockKLEDataBridge = SetupUpdateObjects(out var stubTaskRefRepository, out var removedTaskRef, out var renamedTaskRef);
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockLogger = Substitute.For<ILogger>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, stubTaskRefRepository, mockLogger);
            sut.UpdateKLE(0, 0);
            Assert.Equal(21, stubTaskRefRepository.Get().Count());
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_On_NonEmpty_Repos_Fills_Uuid_On_Existing_TaskRef()
        {
            var mockKLEDataBridge = SetupUpdateObjects(out var stubTaskRefRepository, out var removedTaskRef, out var renamedTaskRef);
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockLogger = Substitute.For<ILogger>();
            // Existing item with no Uuid
            const string existingItemTaskKey = "02.02.00";
            stubTaskRefRepository.Insert(SetupTaskRef(1, "KLE-Emne", existingItemTaskKey, "Bebyggelsens højde- og afstandsforhold i almindelighed"));
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, stubTaskRefRepository, mockLogger);
            sut.UpdateKLE(0, 0);
            Assert.Equal(21, stubTaskRefRepository.Get().Count());
            var sampleTaskRef = stubTaskRefRepository.Get(t => t.TaskKey == existingItemTaskKey).First();
            Assert.Equal(Guid.Parse("f0820080-181a-4ea4-9587-02b86aa13898"), sampleTaskRef.Uuid);
        }
        
        [Fact]
        private void UpdateKLE_Given_Summary_Updates_Both_TaskRef_And_ItProject()
        {
            var mockKLEDataBridge = SetupUpdateObjects(out var stubTaskRefRepository, out var removedTaskRef, out var renamedTaskRef);
            const int itProjectKey = 1;
            var itProject = new ItProject
            {
                Id = itProjectKey,
                TaskRefs = new List<TaskRef> {removedTaskRef}
            };
            removedTaskRef.ItProjects = new List<ItProject> { itProject };
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockLogger = Substitute.For<ILogger>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, stubTaskRefRepository, mockLogger);
            sut.UpdateKLE(0, 0);
            Assert.False(itProject.TaskRefs.Contains(removedTaskRef));
            Assert.Null(removedTaskRef.ItProjects.FirstOrDefault(p => p.Id == itProjectKey));
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_Both_TaskRef_And_ItSystem()
        {
            var mockKLEDataBridge = SetupUpdateObjects(out var stubTaskRefRepository, out var removedTaskRef, out var renamedTaskRef);
            const int itSystemKey = 1;
            var itSystem = new ItSystem
            {
                Id = itSystemKey,
                TaskRefs = new List<TaskRef> { removedTaskRef }
            };
            removedTaskRef.ItSystems = new List<ItSystem> { itSystem };
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockLogger = Substitute.For<ILogger>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, stubTaskRefRepository, mockLogger);
            sut.UpdateKLE(0,0);
            Assert.False(itSystem.TaskRefs.Contains(removedTaskRef));
            Assert.Null(removedTaskRef.ItSystems.FirstOrDefault(p => p.Id == itSystemKey));
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_Both_TaskRef_And_ItSystemUsages()
        {
            var mockKLEDataBridge = SetupUpdateObjects(out var stubTaskRefRepository, out var removedTaskRef, out var renamedTaskRef);
            const int itSystemUsageKey = 1;
            var itSystemUsage = new ItSystemUsage
            {
                Id = itSystemUsageKey,
                TaskRefs = new List<TaskRef> { removedTaskRef }
            };
            removedTaskRef.ItSystemUsages = new List<ItSystemUsage> { itSystemUsage };
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockLogger = Substitute.For<ILogger>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, stubTaskRefRepository, mockLogger);
            sut.UpdateKLE(0,0);
            Assert.False(itSystemUsage.TaskRefs.Contains(removedTaskRef));
            Assert.Null(removedTaskRef.ItSystemUsages.FirstOrDefault(p => p.Id == itSystemUsageKey));
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_Both_TaskRef_And_ItSystemUsageOptOut()
        {
            var mockKLEDataBridge = SetupUpdateObjects(out var stubTaskRefRepository, out var removedTaskRef, out var renamedTaskRef);
            const int itSystemUsagesOptOutKey = 1;
            var itSystemUsage = new ItSystemUsage
            {
                Id = itSystemUsagesOptOutKey,
                TaskRefs = new List<TaskRef> { removedTaskRef }
            };
            removedTaskRef.ItSystemUsagesOptOut = new List<ItSystemUsage> { itSystemUsage };
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockLogger = Substitute.For<ILogger>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, stubTaskRefRepository, mockLogger);
            sut.UpdateKLE(0,0);
            Assert.False(itSystemUsage.TaskRefs.Contains(removedTaskRef));
            Assert.Null(removedTaskRef.ItSystemUsagesOptOut.FirstOrDefault(p => p.Id == itSystemUsagesOptOutKey));
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_Both_TaskRef_And_TaskUsage()
        {
            var mockKLEDataBridge = SetupUpdateObjects(out var stubTaskRefRepository, out var removedTaskRef, out var renamedTaskRef);
            const int taskUsageKey = 1;
            var taskUsage = new TaskUsage
            {
                Id = taskUsageKey,
                TaskRef = removedTaskRef
            };
            removedTaskRef.Usages = new List<TaskUsage> { taskUsage };
            var mockTransactionManager = Substitute.For<ITransactionManager>();
            var mockLogger = Substitute.For<ILogger>();
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTransactionManager, stubTaskRefRepository, mockLogger);
            sut.UpdateKLE(0,0);
            Assert.Null(taskUsage.TaskRef);
            Assert.Null(removedTaskRef.Usages.FirstOrDefault(p => p.Id == taskUsageKey));
        }

        #region Helpers

        private static IKLEDataBridge SetupUpdateObjects(out GenericRepositoryTaskRefStub stubTaskRefRepository, out TaskRef removedTaskRef, out TaskRef renamedTaskRef)
        {
            var mockKLEDataBridge = Substitute.For<IKLEDataBridge>();
            var document = XDocument.Load("./ApplicationServices/KLE/20200106-kle-sample-changes.xml");
            mockKLEDataBridge.GetKLEXMLData().Returns(document);
            removedTaskRef = SetupTaskRef(1, "KLE-Emne", "00.03.01", "Dummy");
            renamedTaskRef = SetupTaskRef(2, "KLE-Emne", "00.03.00", "International virksomhed og EU");
            stubTaskRefRepository = new GenericRepositoryTaskRefStub();
            stubTaskRefRepository.Insert(removedTaskRef);
            stubTaskRefRepository.Insert(renamedTaskRef);
            return mockKLEDataBridge;
        }

        private static TaskRef SetupTaskRef(int id, string kleType, string kleTaskKey, string kleDescription = "")
        {
            return new TaskRef { Id = id, Type = kleType, TaskKey = kleTaskKey, Description = kleDescription };
        }

        #endregion
    }
}
