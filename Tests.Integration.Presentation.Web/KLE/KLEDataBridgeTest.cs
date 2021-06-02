//TODO: https://os2web.atlassian.net/browse/KITOSUDV-1862
//using System;
//using System.Linq;
//using Infrastructure.Services.KLEDataBridge;
//using Xunit;

//namespace Tests.Integration.Presentation.Web.KLE
//{
//    public class KLEDataBridgeTest
//    {
//        [Fact]
//        public void GetKLEXMLData_Returns_Valid_XML()
//        {
//            var sut = new KLEDataBridge();
//            var result = sut.GetAllActiveKleNumbers();
//            var publishingDateXElement = result.Descendants("UdgivelsesDato");
//            DateTime.Parse(publishingDateXElement.First().Value);
//        }
//    }
//}
