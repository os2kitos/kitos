using System.Xml.Linq;

namespace Core.DomainServices.Repositories.KLE
{
    public interface IKLEConverterHelper
    {
        MostRecentKLE ConvertToTaskRefs(XDocument document);
    }
}