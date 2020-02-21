using System.Xml.Linq;

namespace Infrastructure.Services.KLEDataBridge
{
    public interface IKLEDataBridge
    {
        XDocument GetAllActiveKleNumbers();
    }
}