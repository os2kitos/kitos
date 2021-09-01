using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.KLE;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.KLE
{
    public interface IKLEApplicationService
    {
        Result<KLEStatus, OperationFailure> GetKLEStatus();
        Result<IEnumerable<KLEChange>, OperationFailure> GetKLEChangeSummary();
        Result<KLEUpdateStatus, OperationFailure> UpdateKLE(int organizationId);
        /// <summary>
        /// Performs KLE search and responds with a versioned result set
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        Result<(Maybe<DateTime> updateReference, IQueryable<TaskRef> contents), OperationError> SearchKle(params IDomainQuery<TaskRef>[] conditions);

        Result<(Maybe<DateTime> updateReference, TaskRef kle), OperationError> GetKle(Guid kleUuid);
    }
}