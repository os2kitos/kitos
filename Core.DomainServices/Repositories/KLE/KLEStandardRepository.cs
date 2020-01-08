using System;
using System.Globalization;
using System.Linq;
using Infrastructure.Services.KLEDataBridge;

namespace Core.DomainServices.Repositories.KLE
{
    public class KLEStandardRepository : IKLEStandardRepository
    {
        private readonly IKLEDataBridge _kleDataBridge;

        public KLEStandardRepository(IKLEDataBridge kleDataBridge)
        {
            _kleDataBridge = kleDataBridge;
        }

        public KLEStatus GetKLEStatus()
        {
            var kleXmlData = _kleDataBridge.GetKLEXMLData();
            var publishedString = kleXmlData.Descendants("UdgivelsesDato").First().Value;
            var publishedDate = DateTime.Parse(publishedString, CultureInfo.InvariantCulture);
            return new KLEStatus
            {
                UpToDate = DateTime.Now.Date>=publishedDate.Date,
                Published = publishedDate
            };
        }
    }
}
