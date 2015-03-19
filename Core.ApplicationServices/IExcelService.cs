using System;
using System.Collections.Generic;
using System.IO;
using Core.DomainModel;

namespace Core.ApplicationServices
{
    public interface IExcelService
    {
        Stream ExportOrganizationUnits(Stream stream, int organizationId, User kitosUser);
        Stream ExportUsers(Stream stream, int organizationId, User kitosUser);
        void ImportOrganizationUnits(Stream stream, int organizationId, User kitosUser);
        void ImportUsers(Stream stream, int organizationId, User kitosUser);
    }

    public class ExcelImportException : Exception
    {
        public List<ExcelImportError> Errors { get; set; }
    }
}
