using System.Collections.Generic;
using System.IO;
using Core.DomainModel;

namespace Core.ApplicationServices
{
    public interface IMoxService
    {
        Stream Export(Stream stream, int organizationId, User kitosUser);
        IEnumerable<MoxImportError> Import(Stream stream, int organizationId, User kitosUser);
    }
}
