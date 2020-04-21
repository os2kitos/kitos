using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Repositories.KLE;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.KLEDataBridge;
using Moq;
using Serilog;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.KLE
{
    public class KLEStandardRepositoryTest
    {
        private readonly Mock<IKLEDataBridge> _mockKleDataBridge;
        private readonly Mock<ITransactionManager> _mockTransactionManager;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IGenericRepository<ItSystemUsage>> _mockSystemUsageRepository;
        private readonly Mock<IGenericRepository<TaskUsage>> _mockTaskUsageRepository;
        private readonly Mock<IOperationClock> _mockClock;
        private readonly GenericRepositoryTaskRefStub _stubTaskRefRepository;
        private readonly KLEStandardRepository _sut;

        public KLEStandardRepositoryTest()
        {
            _mockKleDataBridge = new Mock<IKLEDataBridge>();
            _mockTransactionManager = new Mock<ITransactionManager>();
            _mockLogger = new Mock<ILogger>();
            _mockSystemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _mockTaskUsageRepository = new Mock<IGenericRepository<TaskUsage>>();
            _stubTaskRefRepository = new GenericRepositoryTaskRefStub();
            _mockClock = new Mock<IOperationClock>();
            _mockClock.Setup(c => c.Now).Returns(DateTime.Now);
            _sut = new KLEStandardRepository(_mockKleDataBridge.Object, _mockTransactionManager.Object, _stubTaskRefRepository, _mockSystemUsageRepository.Object, _mockTaskUsageRepository.Object, _mockClock.Object, _mockLogger.Object);
        }

        [Theory]
        [InlineData("31-10-2019", false)]
        [InlineData("01-01-2020", true)]
        private void GetKLEStatus_Returns_ValidStatus(string lastUpdatedString, bool expectedUpToDate)
        {
            //Arrange
            var document = XDocument.Load("./ApplicationServices/KLE/20200106-kle-only-published-date.xml");
            var expectedPublishDate = DateTime.Parse(document.Descendants("UdgivelsesDato").First().Value, CultureInfo.GetCultureInfo("da-DK"));
            _mockKleDataBridge.Setup(r => r.GetAllActiveKleNumbers()).Returns(document);
            
            //Act
            var result = _sut.GetKLEStatus(DateTime.Parse(lastUpdatedString, CultureInfo.GetCultureInfo("da-DK")));

            //Assert
            Assert.Equal(expectedUpToDate, result.UpToDate);
            Assert.Equal(expectedPublishDate, result.Published);
        }

        [Fact]
        private void GetKLEChangeSummary_Returns_List_Of_Changes()
        {
            //Arrange
            var document = XDocument.Load("./ApplicationServices/KLE/20200106-kle-sample-changes.xml");
            _mockKleDataBridge.Setup(r => r.GetAllActiveKleNumbers()).Returns(document);
            // Removed item examples
            _stubTaskRefRepository.Insert(SetupTaskRef(1, "KLE-Hovedgruppe", "03"));
            _stubTaskRefRepository.Insert(SetupTaskRef(2, "KLE-Gruppe", "00.02" ));
            _stubTaskRefRepository.Insert(SetupTaskRef(3, "KLE-Emne", "00.03.01"));
            // Renamed item example
            _stubTaskRefRepository.Insert(SetupTaskRef(4, "KLE-Emne", "00.03.00", "International virksomhed og EU"));
            // Unchanged item example
            _stubTaskRefRepository.Insert(SetupTaskRef(5, "KLE-Emne", "02.02.00", "Bebyggelsens højde- og afstandsforhold i almindelighed"));

            //Act
            var result = _sut.GetKLEChangeSummary();

            //Assert
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
        private void UpdateKLE_Given_Summary_Updates_Returns_Published_Date()
        {
            SetupUpdateObjects();

            var result = _sut.UpdateKLE(0);

            Assert.Equal(DateTime.Parse("01-11-2019", CultureInfo.GetCultureInfo("da-DK")), result);
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_Renamed_TaskRefs()
        {
            var updateObjects = SetupUpdateObjects();

            _sut.UpdateKLE(0);

            Assert.Equal("HAS BEEN RENAMED", updateObjects.renamedTaskRef.Description);
            Assert.Equal(Guid.Parse("f8d6e719-e050-48d8-89e2-977d0eaba6bb"), updateObjects.renamedTaskRef.Uuid);
            Assert.Equal(DateTime.Today, updateObjects.renamedTaskRef.LastChanged.Date);
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_On_Empty_Repos_Adds_All_TaskRefs()
        {
            var updateObjects = SetupUpdateObjects();

            _sut.UpdateKLE(0);

            Assert.Equal(21, updateObjects.stubTaskRefRepository.Get().Count());
            Assert.All(updateObjects.stubTaskRefRepository.Get(), t => Assert.Equal(DateTime.Today, t.LastChanged.Date));
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_On_NonEmpty_Repos_Fills_Uuid_On_Existing_TaskRef()
        {
            //Arrange
            var updateObjects = SetupUpdateObjects();
            // Existing item with no Uuid
            const string existingItemTaskKey = "02.02.00";
            updateObjects.stubTaskRefRepository.Insert(SetupTaskRef(1, "KLE-Emne", existingItemTaskKey, "Bebyggelsens højde- og afstandsforhold i almindelighed"));

            //Act
            _sut.UpdateKLE( 0);

            //Assert
            Assert.Equal(21, updateObjects.stubTaskRefRepository.Get().Count());
            var sampleTaskRef = updateObjects.stubTaskRefRepository.Get(t => t.TaskKey == existingItemTaskKey).First();
            Assert.Equal(Guid.Parse("f0820080-181a-4ea4-9587-02b86aa13898"), sampleTaskRef.Uuid);
            Assert.Equal(DateTime.Today, sampleTaskRef.LastChanged.Date);

        }
        
        [Fact]
        private void UpdateKLE_Given_Summary_Updates_ItProject()
        {
            //Arrange
            var updateObjects = SetupUpdateObjects();
            const int itProjectKey = 1;
            var itProject = new ItProject
            {
                Id = itProjectKey,
                TaskRefs = new List<TaskRef> {updateObjects.removedTaskRef}
            };
            updateObjects.removedTaskRef.ItProjects = new List<ItProject> { itProject };

            //Act
            _sut.UpdateKLE( 0);

            //Assert
            Assert.False(itProject.TaskRefs.Contains(updateObjects.removedTaskRef));
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_ItSystem()
        {
            //Arrange
            var updateObjects = SetupUpdateObjects();
            const int itSystemKey = 1;
            var itSystem = new ItSystem
            {
                Id = itSystemKey,
                TaskRefs = new List<TaskRef> { updateObjects.removedTaskRef }
            };
            updateObjects.removedTaskRef.ItSystems = new List<ItSystem> { itSystem };

            //Act
            _sut.UpdateKLE(0);

            //Assert
            Assert.False(itSystem.TaskRefs.Contains(updateObjects.removedTaskRef));
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_ItSystemUsages()
        {
            //Arrange
            var updateObjects = SetupUpdateObjects();
            const int itSystemUsageKey = 1;
            var itSystemUsage = new ItSystemUsage
            {
                Id = itSystemUsageKey,
                TaskRefs = new List<TaskRef> { updateObjects.removedTaskRef }
            };
            var itSystemUsages = new List<ItSystemUsage> { itSystemUsage };
            _mockSystemUsageRepository
                .Setup(s => s.GetWithReferencePreload(t => t.TaskRefs))
                .Returns(itSystemUsages.AsQueryable);

            //Act
            _sut.UpdateKLE(0);

            //Assert
            Assert.False(itSystemUsage.TaskRefs.Contains(updateObjects.removedTaskRef));
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_ItSystemUsageOptOut()
        {
            //Arrange
            var updateObjects = SetupUpdateObjects();
            const int itSystemUsagesOptOutKey = 1;
            var itSystemUsage = new ItSystemUsage
            {
                Id = itSystemUsagesOptOutKey,
                TaskRefs = new List<TaskRef> { updateObjects.removedTaskRef }
            };
            updateObjects.removedTaskRef.ItSystemUsagesOptOut = new List<ItSystemUsage> { itSystemUsage };

            //Act
            _sut.UpdateKLE(0);

            //Assert
            Assert.False(itSystemUsage.TaskRefs.Contains(updateObjects.removedTaskRef));
        }

        [Fact]
        private void UpdateKLE_Given_Summary_Updates_TaskUsage()
        {
            //Arrange
            var updateObjects = SetupUpdateObjects();
            const int taskUsageKey = 1;
            var taskUsage = new TaskUsage
            {
                Id = taskUsageKey,
                TaskRef = updateObjects.removedTaskRef
            };
            var taskUsages = new List<TaskUsage> { taskUsage };
            _mockTaskUsageRepository
                .Setup(s => s.GetWithReferencePreload(t => t.TaskRef))
                .Returns(taskUsages.AsQueryable);
            _mockTaskUsageRepository.Setup(s => s.RemoveRange(taskUsages));

            //Act
            _sut.UpdateKLE(0);

            //Assert
            _mockTaskUsageRepository.VerifyAll();
        }

        #region Helpers

        private (
            Mock<IKLEDataBridge> mockKLEDataBridge, 
            Mock<ITransactionManager> mockTransactionManager, 
            Mock<ILogger> mockLogger,
            GenericRepositoryTaskRefStub stubTaskRefRepository, 
            TaskRef removedTaskRef, 
            TaskRef renamedTaskRef) SetupUpdateObjects()
        {
            var document = XDocument.Load("./ApplicationServices/KLE/20200106-kle-sample-changes.xml");
            _mockKleDataBridge.Setup(r => r.GetAllActiveKleNumbers()).Returns(document);
            var removedTaskRef = SetupTaskRef(1, "KLE-Emne", "00.03.01", "Dummy");
            var renamedTaskRef = SetupTaskRef(2, "KLE-Emne", "00.03.00", "International virksomhed og EU");
            _stubTaskRefRepository.Insert(removedTaskRef);
            _stubTaskRefRepository.Insert(renamedTaskRef);
            _mockTransactionManager.Setup(t => t.Begin(It.IsAny<IsolationLevel>())).Returns(new Mock<IDatabaseTransaction>().Object);
            return (_mockKleDataBridge, _mockTransactionManager, _mockLogger, _stubTaskRefRepository, removedTaskRef, renamedTaskRef);
        }

        private static TaskRef SetupTaskRef(int id, string kleType, string kleTaskKey, string kleDescription = "")
        {
            return new TaskRef { Id = id, Type = kleType, TaskKey = kleTaskKey, Description = kleDescription };
        }

        #endregion
    }
}
