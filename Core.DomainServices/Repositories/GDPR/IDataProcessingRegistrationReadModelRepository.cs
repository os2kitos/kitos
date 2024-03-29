﻿using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR.Read;


namespace Core.DomainServices.Repositories.GDPR
{
    public interface IDataProcessingRegistrationReadModelRepository
    {
        DataProcessingRegistrationReadModel Add(DataProcessingRegistrationReadModel newModel);
        Maybe<DataProcessingRegistrationReadModel> GetBySourceId(int sourceId);
        void Update(DataProcessingRegistrationReadModel updatedModel);
        void DeleteBySourceId(int sourceId);
        IQueryable<DataProcessingRegistrationReadModel> GetByOrganizationId(int organizationId);
        void Delete(DataProcessingRegistrationReadModel readModel);
        IQueryable<DataProcessingRegistrationReadModel> GetByUserId(int userId);
        IQueryable<DataProcessingRegistrationReadModel> GetReadModelsMustUpdateToChangeActiveState();
    }
}
