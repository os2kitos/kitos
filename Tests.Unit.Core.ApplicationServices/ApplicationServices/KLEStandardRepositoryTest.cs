using System;
using System.Linq;
using System.Xml.Linq;
using Core.DomainServices.Repositories.KLE;
using Infrastructure.Services.KLEDataBridge;
using NSubstitute;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class KLEStandardRepositoryTest
    {
        [Theory]
        [InlineData("2019-11-01", true)]
        [InlineData("9999-12-31", false)]
        private void GetKLEStatus_Returns_ValidStatus(string currentDate, bool expectedUpToDate)
        {
            var mockKLEDataBridge = Substitute.For<IKLEDataBridge>();
            var document = XDocument.Load("./ApplicationServices/20200106-kle-emneplan-response.xml");
            var publishedDate = DateTime.Parse(currentDate);
            document.Descendants("UdgivelsesDato").First().Value = publishedDate.ToLongDateString();
            mockKLEDataBridge.GetKLEXMLData().Returns(document);
            var sut = new KLEStandardRepository(mockKLEDataBridge);
            var result = sut.GetKLEStatus();
            Assert.Equal(expectedUpToDate, result.UpToDate);
            Assert.Equal(publishedDate, result.Published);
        }
    }
}
