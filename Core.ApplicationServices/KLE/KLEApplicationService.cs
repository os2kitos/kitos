using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.KLE;
using Core.DomainServices.Repositories.TaskRefs;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.KLE
{
    public class KLEApplicationService : IKLEApplicationService
    {
        private readonly IOrganizationalUserContext _organizationalUserContext;
        private readonly IKLEStandardRepository _kleStandardRepository;
        private readonly IKLEUpdateHistoryItemRepository _kleUpdateHistoryItemRepository;
        private readonly ITaskRefRepository _taskRefRepository;

        public KLEApplicationService(IOrganizationalUserContext organizationalUserContext,
            IKLEStandardRepository kleStandardRepository,
            IKLEUpdateHistoryItemRepository kleUpdateHistoryItemRepository,
            ITaskRefRepository taskRefRepository)
        {
            _organizationalUserContext = organizationalUserContext;
            _kleStandardRepository = kleStandardRepository;
            _kleUpdateHistoryItemRepository = kleUpdateHistoryItemRepository;
            _taskRefRepository = taskRefRepository;
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

        public Result<KLEUpdateStatus, OperationFailure> UpdateKLE(int organizationId)
        {
            if (!HasAccess())
            {
                return OperationFailure.Forbidden;
            }
            if (GetKLEStatusFromLastUpdated().UpToDate)
            {
                return OperationFailure.BadInput;
            }

            var publishedDate = _kleStandardRepository.UpdateKLE(organizationId);
            _kleUpdateHistoryItemRepository.Insert(publishedDate);
            return KLEUpdateStatus.Ok;
        }

        public Result<(Maybe<DateTime> updateReference, IQueryable<TaskRef> contents), OperationError> SearchKle(params IDomainQuery<TaskRef>[] conditions)
        {
            var lastUpdated = _kleUpdateHistoryItemRepository.GetLastUpdated();
            var taskRefs = _taskRefRepository.Query(conditions);
            return (lastUpdated, taskRefs);
        }

        public Result<(Maybe<DateTime> updateReference, TaskRef kle), OperationError> GetKle(Guid kleUuid)
        {
            return
                SearchKle()
                    .Select(x => (x.updateReference, x.contents.ByUuid(kleUuid).FromNullable()))
                    .Match
                    (
                        result => result
                            .Item2
                            .Match<Result<(Maybe<DateTime> updateReference, TaskRef kle), OperationError>>
                            (
                                foundKle => (result.updateReference, foundKle),
                                () => new OperationError(OperationFailure.NotFound)
                            ),
                        error => error
                    );
        }

        private bool HasAccess()
        {
            return _organizationalUserContext.IsGlobalAdmin();
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
