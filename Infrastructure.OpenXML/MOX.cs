using System;
using System.Data;
using System.IO;
using System.Linq;
using Core.ApplicationServices;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DataTable = System.Data.DataTable;

namespace Infrastructure.OpenXML
{
    public class Mox : IMox
    {

        public DataSet Import(Stream stream)
        {
            DataSet dataSet = new DataSet();
            
            //Open document
            var spreadsheetDocument = SpreadsheetDocument.Open(stream, true);

            //Open WorkbookPart
            var workbookPart = spreadsheetDocument.WorkbookPart;
            
            //Open Sheets
            var sheets = workbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

            foreach (var sheet in sheets)
            {
                //if (sheet.Name != "Organisation")
                //{
                //    continue;
                //}
                
                var dataTable = new DataTable() { TableName = sheet.Name };
                
                var relationshipId = sheet.Id.Value;

                var worksheetPart = (WorksheetPart) workbookPart.GetPartById(relationshipId);
                var workSheet = worksheetPart.Worksheet;
                var sheetData = workSheet.GetFirstChild<SheetData>();

                var rows = sheetData.Descendants<Row>();

                var columns = rows.Max(x => x.Descendants<Cell>().Count());

                for (int i = 0; i < columns; i++)
                {
                    dataTable.Columns.Add();
                }

                foreach (var row in rows) //Includes header row
                {
                    if(row.RowIndex < 8)
                        continue;
    
                    var dataRow = dataTable.NewRow();

                    for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                    {
                        dataRow[i] = GetCellValue(spreadsheetDocument, row.Descendants<Cell>().ElementAt(i));
                    }

                    dataTable.Rows.Add(dataRow);
                }

                //dataTable.Rows.RemoveAt(0); // removes header row

                dataSet.Tables.Add(dataTable);
            }

            return dataSet;
        }

        private string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            var stringTablePart = document.WorkbookPart.SharedStringTablePart;

            var value = "";

            if(cell.CellValue != null)
                value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
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
