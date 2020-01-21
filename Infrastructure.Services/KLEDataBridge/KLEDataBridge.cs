using System.Configuration;
using System.IO;
using System.Net;
using System.Xml.Linq;
using Infrastructure.Services.Properties;

namespace Infrastructure.Services.KLEDataBridge
{
    public class KLEDataBridge : IKLEDataBridge
    {
        public XDocument GetKLEXMLData()
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead(Settings.Default.KLEOnlineUrl + "/emneplan?uuid=true"))
            using (var reader = new StreamReader(stream))
            {
                return XDocument.Load(reader);
            }
        }
    }
}
