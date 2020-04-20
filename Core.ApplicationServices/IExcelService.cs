using System;
using System.Collections.Generic;
using System.IO;

namespace Core.ApplicationServices
{
    public interface IExcelService
    {
        Stream ExportOrganizationUnits(Stream stream, int organizationId);
        Stream ExportUsers(Stream stream, int organizationId);
        void ImportOrganizationUnits(Stream stream, int organizationId);
        void ImportUsers(Stream stream, int organizationId);
        void ExportItContracts(Stream stream, int organizationId);
        void ImportItContracts(Stream stream, int organizationId);
    }

    public class ExcelImportException : Exception
    {
        public IEnumerable<ExcelImportError> Errors { get; set; }
    }
}
