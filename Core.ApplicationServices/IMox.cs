using System.Data;
using System.IO;
using System.Linq;

namespace Core.ApplicationServices
{
    public interface IMox
    {
        IOrderedEnumerable<string> Import(Stream stream);

        Stream Export(DataSet data, Stream stream);
    }
}