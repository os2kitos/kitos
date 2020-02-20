using System.Xml.Linq;
using Core.DomainModel.KLE;

namespace Core.DomainServices.Repositories.KLE
{
    public interface IKLEConverterHelper
    {
        KLEMostRecent ConvertToTaskRefs(XDocument document);
    }
}