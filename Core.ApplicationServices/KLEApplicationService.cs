using System.Collections.Generic;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.Result;
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

        public Result<KLEStatus, OperationFailure> GetKLEStatus()
        {
            if (!_organizationalUserContext.HasRole(OrganizationRole.GlobalAdmin))
            {
                return Result<KLEStatus, OperationFailure>.Failure(OperationFailure.Forbidden);
            }
            return Result<KLEStatus, OperationFailure>.Success(GetKLEStatusFromLastUpdated());
        }

        public Result<IEnumerable<KLEChange>, OperationFailure> GetKLEChangeSummary()
        {
            if (!_organizationalUserContext.HasRole(OrganizationRole.GlobalAdmin))
            {
                return Result<IEnumerable<KLEChange>, OperationFailure>.Failure(OperationFailure.Forbidden);
            }
            return Result<IEnumerable<KLEChange>, OperationFailure>.Success(_kleStandardRepository.GetKLEChangeSummary());
        }

        public Result<KLEUpdateStatus, OperationFailure> UpdateKLE()
        {
            if (!_organizationalUserContext.HasRole(OrganizationRole.GlobalAdmin))
            {
                return Result<KLEUpdateStatus, OperationFailure>.Failure(OperationFailure.Forbidden);
            }
            if (GetKLEStatusFromLastUpdated().UpToDate)
            {
                return Result<KLEUpdateStatus, OperationFailure>.Failure(OperationFailure.BadInput);
            }
            var publishedDate = _kleStandardRepository.UpdateKLE(_organizationalUserContext.UserId, _organizationalUserContext.ActiveOrganizationId);
            _kleUpdateHistoryItemRepository.Insert(publishedDate, _organizationalUserContext.UserId);
            return Result<KLEUpdateStatus, OperationFailure>.Success(KLEUpdateStatus.Ok);
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
