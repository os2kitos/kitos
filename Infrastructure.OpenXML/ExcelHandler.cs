using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Core.ApplicationServices;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;

namespace Infrastructure.OpenXML
{
    public class ExcelHandler : IExcelHandler
    {
        public DataSet Import(Stream stream)
        {
            var dataSet = new DataSet();

            // open document
            var spreadsheetDocument = SpreadsheetDocument.Open(stream, true);

            // open WorkbookPart
            var workbookPart = spreadsheetDocument.WorkbookPart;

            // open Sheets
            var sheets = workbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

            foreach (var sheet in sheets)
            {
                var dataTable = new DataTable { TableName = sheet.Name };
                var relationshipId = sheet.Id.Value;
                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(relationshipId);
                var workSheet = worksheetPart.Worksheet;
                var sheetData = workSheet.GetFirstChild<SheetData>();
                var rows = sheetData.Descendants<Row>().ToList();
                var numColumns = rows.Max(x => x.Descendants<Cell>().Count());

                for (var i = 0; i < numColumns; i++)
                {
                    dataTable.Columns.Add();
                }

                foreach (var row in rows)
                {
                    // skip header row
                    if (row.RowIndex < 2)
                        continue;

                    var dataRow = dataTable.NewRow();

                    var cells = GetCells(row, numColumns);

                    var i = 0;
                    var rowHasValue = false;

                    foreach (var cell in cells)
                    {
                        var cellValue = GetCellValue(spreadsheetDocument, cell);
                        dataRow[i] = cellValue;

                        if (!string.IsNullOrWhiteSpace(cellValue))
                        {
                            rowHasValue = true;
                        }

                        i++;
                    }

                    if (rowHasValue)
                    {
                        dataTable.Rows.Add(dataRow);
                    }

                }
                dataSet.Tables.Add(dataTable);
            }
            return dataSet;
        }

        /// <summary>
        /// Gets a list of cells with the specified length.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="numColumns">The number of columns.</param>
        /// <returns>returns a list of cells of length <see cref="numColumns"/>.</returns>
        /// <remarks>
        /// if there's a blank cell between two non blank cell, this list will properly reflect that
        /// by inserting empty cells the right places.
        /// likewise, if the row doesn't have @numColumns cells, empty cells will be appended.        
        /// </remarks>
        private static IEnumerable<Cell> GetCells(Row row, int numColumns)
        {
            var result = new List<Cell>();

            var i = 0;
            foreach (var cell in row.Descendants<Cell>())
            {
                var columnName = GetColumnName(cell.CellReference);
                // column index is the real index of the cell
                var columnIndex = ConvertColumnNameToNumber(columnName);

                // for every missing cell between the previous and the current index, add a new cell
                for (; i < columnIndex; i++)
                {
                    result.Add(new Cell());
                }

                // then add the current cell
                result.Add(cell);
                i++;
            }

            // add all remaining cells until we reach last column
            for (; i < numColumns; i++)
            {
                result.Add(new Cell());
            }

            return result;
        }

        /// <summary>
        /// Given a cell name, parses the specified cell to get the column name.
        /// See http://stackoverflow.com/questions/3837981/reading-excel-open-xml-is-ignoring-blank-cells
        /// </summary>
        /// <param name="cellReference">Address of the cell (ie. B2)</param>
        /// <returns>Column Name (ie. B)</returns>
        private static string GetColumnName(string cellReference)
        {
            // match the column name portion of the cell name.
            var regex = new Regex("[A-Za-z]+");
            var match = regex.Match(cellReference);

            return match.Value;
        }

        /// <summary>
        /// Given just the column name (no row index),
        /// it will return the zero based column index.
        /// See http://stackoverflow.com/questions/3837981/reading-excel-open-xml-is-ignoring-blank-cells
        /// </summary>
        /// <param name="columnName">Column Name (ie. A or AB)</param>
        /// <returns>Zero based index if the conversion was successful</returns>
        /// <exception cref="ArgumentException">thrown if the given string
        /// contains characters other than uppercase letters</exception>
        private static int ConvertColumnNameToNumber(string columnName)
        {
            var alpha = new Regex("^[A-Z]+$");
            if (!alpha.IsMatch(columnName)) throw new ArgumentException();

            var colLetters = columnName.ToCharArray();
            Array.Reverse(colLetters);

            var convertedValue = 0;
            for (var i = 0; i < colLetters.Length; i++)
            {
                var letter = colLetters[i];
                var current = i == 0 ? letter - 65 : letter - 64; // ASCII 'A' = 65
                convertedValue += current * (int)Math.Pow(26, i);
            }

            return convertedValue;
        }

        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            if (cell == null) return "";

            var stringTablePart = document.WorkbookPart.SharedStringTablePart;
            var value = "";

            if (cell.CellValue != null)
                value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            return value;
        }

        public Stream Export(DataSet data, Stream stream)
        {
            // open document
            var spreadsheetDocument = SpreadsheetDocument.Open(stream, true);

            // open a WorkbookPart
            var workbookPart = spreadsheetDocument.WorkbookPart;

            // loop through data
            foreach (DataTable table in data.Tables)
            {
                var id = workbookPart.Workbook.Descendants<Sheet>().First(x => x.Name == table.TableName).Id;
                var workSheetPart = (WorksheetPart)workbookPart.GetPartById(id);
                var sheetData = workSheetPart.Worksheet.GetFirstChild<SheetData>();

                // append the locked cell format to the stylesheet
                var stylesPart = workbookPart.GetPartsOfType<WorkbookStylesPart>().FirstOrDefault() ?? workbookPart.AddNewPart<WorkbookStylesPart>();
                var stylesheet = stylesPart.Stylesheet;

                var fonts = stylesheet.GetFirstChild<Fonts>();
                fonts.AppendChild(new Font
                {
                    Italic = new Italic(),
                    Color = new Color() {Rgb = "FF7F7F7F"}
                });
                fonts.Count++;

                // we just appended a lock cell style, ergo it must be the last!
                var fontId = fonts.Count - 1; // the fonts index starts at 0 so subtract 1
                var lockFormat = new CellFormat
                {
                    ApplyProtection = true,
                    Protection = new Protection { Locked = true },
                    FontId = fontId
                };

                var cellFormats = stylesheet.GetFirstChild<CellFormats>();
                cellFormats.AppendChild(lockFormat);
                cellFormats.Count++;

                // we just appended a lock cell style, ergo it must be the last!
                var cellLockStyleIndex = cellFormats.Count - 1; // the style index starts at 0 so subtract 1

                // delete all rows except for the header row
                var headerRow = sheetData.Elements<Row>().FirstOrDefault();
                sheetData.RemoveAllChildren();
                sheetData.AppendChild(headerRow);

                foreach (DataRow row in table.Rows)
                {
                    var newRow = new Row();

                    foreach (DataColumn column in table.Columns)
                    {
                        var newCell = new Cell
                        {
                            CellValue = new CellValue(row[column].ToString()),
                            DataType = CellValues.String,
                            StyleIndex = cellLockStyleIndex //locked style
                        };

                        //TODO: always int?
                        int t;
                        newCell.DataType = int.TryParse(row[column].ToString(), out t) ? CellValues.Number : CellValues.String;
                        newRow.AppendChild(newCell);
                    }
                    sheetData.AppendChild(newRow);
                }
            }
            spreadsheetDocument.Close();

            return stream;
        }
    }
}
