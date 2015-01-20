using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.Data;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DataTable = DocumentFormat.OpenXml.Drawing.Charts.DataTable;

namespace Infrastructure.OpenXML
{

    public interface IMox
    {
        IOrderedEnumerable<string> Import(Stream stream);

        Stream Export(DataSet data);
    }

    public class Mox : IMox
    {

        public IOrderedEnumerable<string> Import(Stream stream)
        {
            var spreadsheetDocument = SpreadsheetDocument.Open(stream, true);
            
            throw new NotImplementedException();
        }

        public Stream Export(DataSet data)
        {
            var stream = new MemoryStream();

            //Create document
            var spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true);

            //Add a WorkbookPart to the document
            var workbookPart = spreadsheetDocument.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            //Add a WorksheetPart to the WorkbookPart
            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            //Add Sheets to the Workbook
            var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            //Loop through data
            foreach (System.Data.DataTable table in data.Tables)
            {
                
                var newSheet = new Sheet()
                {
                    Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = table.TableName
                };

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

                    newSheet.AppendChild(newRow);
                }

                sheets.Append(newSheet);
            }

            workbookPart.Workbook.Save(); //TODO: Test if nessesary?
            spreadsheetDocument.Close(); //TODO: Make sure this dosn't clear stream

            return stream;
        }
    }

}
