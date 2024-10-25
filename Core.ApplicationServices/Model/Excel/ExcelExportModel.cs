using System.IO;

namespace Core.ApplicationServices.Model.Excel
{
    public class ExcelExportModel
    {
        public ExcelExportModel(MemoryStream memoryStream, string fileName)
        {
            MemoryStream = memoryStream;
            FileName = fileName;
        }

        public MemoryStream MemoryStream { get; set; }
        public string FileName { get; set; }
    }
}
