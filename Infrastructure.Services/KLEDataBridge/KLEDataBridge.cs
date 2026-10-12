using System.IO;
using System.Net.Http;
using System.Xml.Linq;

namespace Infrastructure.Services.KLEDataBridge
{
    public class KLEDataBridge : IKLEDataBridge
    {
        private readonly string _kleOnlineUrl;

        public KLEDataBridge(string kleOnlineUrl)
        {
            _kleOnlineUrl = kleOnlineUrl;
        }

        public XDocument GetAllActiveKleNumbers()
        {
            using var client = new HttpClient();
            using var stream = client.GetStreamAsync(_kleOnlineUrl.TrimEnd('/') + "/emneplan").GetAwaiter().GetResult();
            using var reader = new StreamReader(stream);
            return XDocument.Load(reader);
        }
    }
}
