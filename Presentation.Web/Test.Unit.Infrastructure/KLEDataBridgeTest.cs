using System;
using System.Xml.Linq;
using Infrastructure.Services.KLEDataBridge;
using Xunit;

namespace Test.Unit.Infrastructure
{
    public class KLEDataBridgeTest
    {
        [Fact]
        void GetKLEXMLData_Returns_Most_Current_DataSet()
        {
            var sut = new KLEDataBridge();
            XDocument result = sut.GetKLEXMLData();
            var publishingDate = result.Element("UdgivelsesDato");
            DateTime.Parse(publishingDate.Value);
        }
    }
}
