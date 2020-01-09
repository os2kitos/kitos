using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Repositories.KLE;
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
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTaskRefRepository);
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
                // Missing item examples
                SetupTaskRef("KLE-Hovedgruppe", "03"),
                SetupTaskRef("KLE-Gruppe", "00.02" ),
                SetupTaskRef("KLE-Emne", "00.03.01"),
                // Renamed item example
                SetupTaskRef("KLE-Emne", "00.03.00", "International virksomhed og EU"),
                // Unchanged item example
                SetupTaskRef("KLE-Emne", "02.02.00", "Bebyggelsens højde- og afstandsforhold i almindelighed"),
            };
            mockTaskRefRepository.Get().Returns(taskRefs);
            var sut = new KLEStandardRepository(mockKLEDataBridge, mockTaskRefRepository);
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

        private static TaskRef SetupTaskRef(string kleType, string kleTaskKey, string kleDescription = "")
        {
            return new TaskRef { Type = kleType, TaskKey = kleTaskKey, Description = kleDescription };
        }
    }
}
