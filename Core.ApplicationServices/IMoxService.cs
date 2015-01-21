using System.IO;
using Core.DomainModel;

namespace Core.ApplicationServices
{
    public interface IMoxService
    {
        Stream Export(Stream stream, int organizationId, User kitosUser);
        void Import(Stream stream, int organizationId, User kitosUser);
    }
}
