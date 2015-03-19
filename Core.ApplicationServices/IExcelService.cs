using System;
using System.Collections.Generic;
using System.IO;
using Core.DomainModel;

namespace Core.ApplicationServices
{
    public interface IExcelService
    {
        Stream Export(Stream stream, int organizationId, User kitosUser);
        Stream ExportUsers(Stream stream, int organizationId, User kitosUser);
        void Import(Stream stream, int organizationId, User kitosUser);
        void ImportUsers(Stream stream, int organizationId, User kitosUser);
    }

    public class ExcelImportException : Exception
    {
        public List<ExcelImportError> Errors { get; set; }
    }
}
