using System.IO;
using System.Net;
using System.Xml.Linq;

namespace Infrastructure.Services.KLEDataBridge
{
    public class KLEDataBridge : IKLEDataBridge
    {
        public XDocument GetKLEXMLData()
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead("http://api.kle-online.dk/resources/kle/emneplan"))
            using (var reader = new StreamReader(stream))
            {
                return XDocument.Load(reader);
            }
        }
    }
}
