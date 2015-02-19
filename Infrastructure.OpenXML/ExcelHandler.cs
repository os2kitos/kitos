using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Core.ApplicationServices;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DataTable = System.Data.DataTable;

namespace Infrastructure.OpenXML
{
    public class ExcelHandler : IExcelHandler
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
               
                var dataTable = new DataTable() { TableName = sheet.Name };
                
                var relationshipId = sheet.Id.Value;

                var worksheetPart = (WorksheetPart) workbookPart.GetPartById(relationshipId);
                var workSheet = worksheetPart.Worksheet;
                var sheetData = workSheet.GetFirstChild<SheetData>();

                var rows = sheetData.Descendants<Row>();

                var columns = rows.Max(x => x.Descendants<Cell>().Count());

                for (var i = 0; i < columns; i++)
                {
                    dataTable.Columns.Add();
                }

                foreach (var row in rows)
                {
                    //skip header row
                    if(row.RowIndex < 2)
                        continue;
    
                    var dataRow = dataTable.NewRow();

                    var hasValue = false;
                    for(var i = =; i < row.Co)

                    for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                    {
                        var cellValue = GetCellValue(spreadsheetDocument, row.Descendants<Cell>().ElementAt(i));
                        dataRow[i] = cellValue;
                        if (!string.IsNullOrWhiteSpace(cellValue))
                        {
                            hasValue = true;
                        }
                    }

                    if (hasValue)
                    {
                        dataTable.Rows.Add(dataRow);
                    }
                    
                }

                //dataTable.Rows.RemoveAt(0); // removes header row?
                
                dataSet.Tables.Add(dataTable);
                
            }

            return dataSet;
        }

        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
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

                /*
                 //cell format is a kind of style that can be applied to cells 
                var unlockedCellFormat = new CellFormat()
                {
                    ApplyProtection = false,
                    Protection = new Protection() { Locked = false }
                };
                
                // append the unlocked cell format to the stylesheet
                var unlockedStyle = workbookPart.AddNewPart<WorkbookStylesPart>();
                unlockedStyle.Stylesheet.CellFormats.AppendChild<CellFormat>(unlockedCellFormat);
                unlockedStyle.Stylesheet.Save();
                 
                var lockedCellFormat = new CellFormat()
                {
                    ApplyProtection = true,
                    Protection = new Protection() { Locked = true }
                };


                // append the locked cell format to the stylesheet
                var workbookStylesPart = workbookPart.GetPartsOfType<WorkbookStylesPart>().FirstOrDefault() ?? workbookPart.AddNewPart<WorkbookStylesPart>();
                workbookStylesPart.Stylesheet.CellFormats.AppendChild<CellFormat>(lockedCellFormat);
                workbookStylesPart.Stylesheet.Save();
                var lockedStyleIndex = UInt32Value.FromUInt32((uint)workbookStylesPart.Stylesheet.CellFormats.Count());
                 */

                //Delete all rows except for the header row
                var headerRow = sheetData.Elements<Row>().FirstOrDefault();
                sheetData.RemoveAllChildren();
                sheetData.AppendChild(headerRow);

                foreach (DataRow row in table.Rows)
                {
                    var newRow = new Row();// { RowIndex = rowIndex };

                    foreach (DataColumn column in table.Columns)
                    {

                        var newCell = new Cell()
                        {
                            CellValue = new CellValue(row[column].ToString()),
                            DataType = CellValues.String,
                            //StyleIndex = lockedStyleIndex //locked style
                        };
                        
                        //TODO: Altid int?
                        int t;
                        newCell.DataType = int.TryParse(row[column].ToString(), out t) ? CellValues.Number : CellValues.String;

                        newRow.AppendChild(newCell);
                    }
                    sheetData.AppendChild(newRow);
                }


                /* disabled until more information on the subject is found - or at least until i can test it properly
                //this is also necessary to enable protection of cells. 
                //a strong password is not really necessary for our application..... afterall this is not a security feature,
                //but more of a UX thing - to avoid that the user accidentally edits stuff he can't
                var sheetProtection = new SheetProtection();
                sheetProtection.Password = "password";

                //really, i have no idea what this means.......... see http://stackoverflow.com/questions/20257842/read-only-or-lock-the-particular-cells-or-rows-using-open-xml-sdk
                sheetProtection.Sheet = true;
                sheetProtection.Objects = true;
                sheetProtection.Scenarios = true;         

                workSheetPart.Worksheet.InsertAfter(sheetProtection, sheetData);
                workSheetPart.Worksheet.Save();
                 */
            }
            

            workbookPart.Workbook.Save(); //TODO: Test if nessesary?
            spreadsheetDocument.Close(); //TODO: Make sure this dosn't clear stream

            return stream;
        }
    }
}
