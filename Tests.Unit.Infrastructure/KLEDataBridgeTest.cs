using System;
using System.Linq;
using Infrastructure.Services.KLEDataBridge;
using Xunit;

namespace Tests.Unit.Infrastructure
{
    public class KLEDataBridgeTest
    {
        [Fact]
        private void GetKLEXMLData_Returns_Valid_XML()
        {
            var sut = new KLEDataBridge();
            var result = sut.GetKLEXMLData();
            var publishingDateXElement = result.Descendants("UdgivelsesDato");
            DateTime.Parse(publishingDateXElement.First().Value);
        }
    }
}
