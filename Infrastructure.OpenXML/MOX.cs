using System;
using System.Data;
using System.IO;
using System.Linq;
using Core.ApplicationServices;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Infrastructure.OpenXML
{
    public class Mox : IMox
    {

        public IOrderedEnumerable<string> Import(Stream stream)
        {
            SpreadsheetDocument.Open(stream, true);

            throw new NotImplementedException();
        }

        public Stream Export(DataSet data, Stream stream)
        {
            //Open document
            var spreadsheetDocument = SpreadsheetDocument.Open(stream, true);

            //Open a WorkbookPart
            var workbookPart = spreadsheetDocument.WorkbookPart;

            //Loop through data
            foreach (DataTable table in data.Tables)
            {
                var id = workbookPart.Workbook.Descendants<Sheet>().First(x => x.Name == table.TableName).Id;
                var workSheetPart = (WorksheetPart)workbookPart.GetPartById(id);
                var sheetData = workSheetPart.Worksheet.GetFirstChild<SheetData>();

                //var lastRow = sheetData.Elements<Row>().LastOrDefault();
                //var lastRowIndex = lastRow.RowIndex + 1;

                //if (lastRow != null)
                //{

                //} 

                foreach (DataRow row in table.Rows)
                {
                    var newRow = new Row();

                    foreach (DataColumn column in table.Columns)
                    {
                        var newCell = new Cell()
                        {
                            CellValue = new CellValue(row[column].ToString()),
                            DataType = CellValues.String
                        };

                        newRow.AppendChild(newCell);
                    }

                    sheetData.AppendChild(newRow);

                }

            }

            workbookPart.Workbook.Save(); //TODO: Test if nessesary?
            spreadsheetDocument.Close(); //TODO: Make sure this dosn't clear stream

            return stream;
        }
    }
}
