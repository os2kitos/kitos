using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage.GDPR;

namespace Core.ApplicationServices.SystemUsage.GDPR
{
    public interface IGDPRExportService
    {
        Result<IEnumerable<GDPRExportReport>, OperationError> GetGDPRData(int organizationId);

        Result<IEnumerable<GDPRExportReport>, OperationError> GetGDPRDataByUuid(Guid organizationUuid);
    }
}
