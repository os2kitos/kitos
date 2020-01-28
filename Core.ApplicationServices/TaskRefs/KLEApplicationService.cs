using System.Collections.Generic;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.KLE;

namespace Core.ApplicationServices.TaskRefs
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

        public Result<KLEStatus, OperationFailure> GetKLEStatus()
        {
            if (!HasAccess())
            {
                return OperationFailure.Forbidden;
            }
            return GetKLEStatusFromLastUpdated();
        }

        public Result<IEnumerable<KLEChange>, OperationFailure> GetKLEChangeSummary()
        {
            if (!HasAccess())
            {
                return OperationFailure.Forbidden;
            }
            return Result<IEnumerable<KLEChange>, OperationFailure>.Success(_kleStandardRepository.GetKLEChangeSummary());
        }

        public Result<KLEUpdateStatus, OperationFailure> UpdateKLE()
        {
            if (!HasAccess())
            {
                return OperationFailure.Forbidden;
            }
            if (GetKLEStatusFromLastUpdated().UpToDate)
            {
                return OperationFailure.BadInput;
            }
            var publishedDate = _kleStandardRepository.UpdateKLE(_organizationalUserContext.UserId, _organizationalUserContext.ActiveOrganizationId);
            _kleUpdateHistoryItemRepository.Insert(publishedDate, _organizationalUserContext.UserId);
            return KLEUpdateStatus.Ok;
        }

        private bool HasAccess()
        {
            return _organizationalUserContext.HasRole(OrganizationRole.GlobalAdmin);
        }

        #region Helpers

        private KLEStatus GetKLEStatusFromLastUpdated()
        {
            var lastUpdated = _kleUpdateHistoryItemRepository.GetLastUpdated();
            var status = _kleStandardRepository.GetKLEStatus(lastUpdated);
            return status;
        }

        #endregion
    }
}
