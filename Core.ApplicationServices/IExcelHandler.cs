using System.Data;
using System.IO;

namespace Core.ApplicationServices
{
    public interface IExcelHandler
    {
        DataSet Import(Stream stream);

        Stream Export(DataSet data, Stream stream);
    }
}
