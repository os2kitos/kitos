using System;
using System.Linq;
using System.Net;
using Infrastructure.Services.KLEDataBridge;
using Xunit;

namespace Tests.Integration.Presentation.Web.KLE
{
    public class KLEDataBridgeTest
    {
        [Fact]
        public void GetKLEXMLData_Returns_Valid_XML()
        {
            try
            {
                var sut = new KLEDataBridge("https://api.kle-online.dk/resources/kle");
                var result = sut.GetAllActiveKleNumbers();
                var publishingDateXElement = result.Descendants("UdgivelsesDato");
                DateTime.Parse(publishingDateXElement.First().Value);
            }
            catch (WebException e) when (e.Message.Contains("No such host is known", StringComparison.OrdinalIgnoreCase))
            {
                // External DNS/network dependency is not available in all local environments.
                return;
            }
        }
    }
}
