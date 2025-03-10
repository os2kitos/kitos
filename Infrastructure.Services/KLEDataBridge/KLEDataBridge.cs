using System.IO;
using System.Net;
using System.Xml.Linq;
using Infrastructure.Services.Properties;

namespace Infrastructure.Services.KLEDataBridge
{
    public class KLEDataBridge : IKLEDataBridge
    {
        public XDocument GetAllActiveKleNumbers()
        {
            using var client = new WebClient();
            using var stream = client.OpenRead(Settings.Default.KLEOnlineUrl?.TrimEnd('/') + "/emneplan");
            using var reader = new StreamReader(stream);
            return XDocument.Load(reader);
        }
    }
}
