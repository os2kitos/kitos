using System.Collections.Generic;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.KLE;

namespace Core.ApplicationServices
{
    public class KLEApplicationService : IKLEApplicationService
    {
        private readonly IOrganizationalUserContext _organizationalUserContext;
        private readonly IKLEStandardRepository _kleStandardRepository;

        public KLEApplicationService(IOrganizationalUserContext organizationalUserContext,
            IKLEStandardRepository kleStandardRepository)
        {
            _organizationalUserContext = organizationalUserContext;
            _kleStandardRepository = kleStandardRepository;
        }

        public Result<OperationResult, KLEStatus> GetKLEStatus()
        {
            if (!_organizationalUserContext.HasRole(OrganizationRole.GlobalAdmin))
            {
                return Result<OperationResult, KLEStatus>.Fail(OperationResult.Forbidden);
            }
            return Result<OperationResult, KLEStatus>.Ok(_kleStandardRepository.GetKLEStatus());
        }

        public Result<OperationResult, IEnumerable<KLEChange>> GetKLEChangeSummary()
        {
            if (!_organizationalUserContext.HasRole(OrganizationRole.GlobalAdmin))
            {
                return Result<OperationResult, IEnumerable<KLEChange>>.Fail(OperationResult.Forbidden);
            }
            return Result<OperationResult, IEnumerable<KLEChange>>.Ok(_kleStandardRepository.GetKLEChangeSummary());
        }
    }
}
