using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.SystemUsage.GDPR
{
    public interface IGDPRExportService
    {
        Result<IEnumerable<GDPRExportReport>, OperationError> GetGDPRData(int organizationId);
    }
}
