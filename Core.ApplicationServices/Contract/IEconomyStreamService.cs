using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract;

namespace Core.ApplicationServices.Contract
{
    public interface IEconomyStreamService
    {
        Maybe<OperationError> Delete(int id);
        Maybe<OperationError> DeleteRange(IEnumerable<int> ids);
        Maybe<OperationError> DeleteRange(IEnumerable<EconomyStream> entities);
        IEnumerable<EconomyStream> GetEconomyStreams(ItContract contract);
        IEnumerable<EconomyStream> GetInternalEconomyStreams(ItContract contract);
        IEnumerable<EconomyStream> GetExternalEconomyStreams(ItContract contract);
    }
}
