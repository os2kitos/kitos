using System;
using System.Collections.Generic;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.KLE;

namespace Core.ApplicationServices
{
    public class KLEApplicationService : IKLEApplicationService
    {
        private readonly IOrganizationalUserContext _organizationalUserContext;
        private readonly IKLEStandardRepository _kleStandardRepository;
        private readonly IKLEUpdateHistoryItemRepository _kleUpdateHistoryItemRepository;

        public KLEApplicationService(IOrganizationalUserContext organizationalUserContext,
            IKLEStandardRepository kleStandardRepository,
            IKLEUpdateHistoryItemRepository kleUpdateHistoryItemRepository)
        {
            _organizationalUserContext = organizationalUserContext;
            _kleStandardRepository = kleStandardRepository;
            _kleUpdateHistoryItemRepository = kleUpdateHistoryItemRepository;
        }

        public Result<OperationResult, KLEStatus> GetKLEStatus()
        {
            if (!_organizationalUserContext.HasRole(OrganizationRole.GlobalAdmin))
            {
                return Result<OperationResult, KLEStatus>.Fail(OperationResult.Forbidden);
            }
            var lastUpdated = _kleUpdateHistoryItemRepository.GetLastUpdated();
            return Result<OperationResult, KLEStatus>.Ok(_kleStandardRepository.GetKLEStatus(lastUpdated));
        }

        public Result<OperationResult, IEnumerable<KLEChange>> GetKLEChangeSummary()
        {
            if (!_organizationalUserContext.HasRole(OrganizationRole.GlobalAdmin))
            {
                return Result<OperationResult, IEnumerable<KLEChange>>.Fail(OperationResult.Forbidden);
            }
            return Result<OperationResult, IEnumerable<KLEChange>>.Ok(_kleStandardRepository.GetKLEChangeSummary());
        }

        public Result<OperationResult, KLEUpdateStatus> UpdateKLE()
        {
            if (!_organizationalUserContext.HasRole(OrganizationRole.GlobalAdmin))
            {
                return Result<OperationResult, KLEUpdateStatus>.Fail(OperationResult.Forbidden);
            }

            var publishedDate = _kleStandardRepository.UpdateKLE(_organizationalUserContext.UserId, _organizationalUserContext.ActiveOrganizationId);
            _kleUpdateHistoryItemRepository.Insert(publishedDate.ToString("dd-MM-yyyy"), _organizationalUserContext.UserId);
            return Result<OperationResult, KLEUpdateStatus>.Ok(KLEUpdateStatus.Ok);
        }
    }
}
