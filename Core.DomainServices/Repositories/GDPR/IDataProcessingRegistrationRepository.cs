﻿using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;


namespace Core.DomainServices.Repositories.GDPR
{
    public interface IDataProcessingRegistrationRepository
    {
        DataProcessingRegistration Add(DataProcessingRegistration newRegistration);
        bool DeleteById(int id);
        void Update(DataProcessingRegistration dataProcessingRegistration);
        Maybe<DataProcessingRegistration> GetById(int id);
        IQueryable<DataProcessingRegistration> GetDataProcessingRegistrationsFromOrganization(int organizationId);
        IQueryable<DataProcessingRegistration> GetBySystemId(int systemId);
        IQueryable<DataProcessingRegistration> Search(int organizationId, Maybe<string> exactName);
        IQueryable<DataProcessingRegistration> GetByDataProcessorId(int organizationId);
        IQueryable<DataProcessingRegistration> GetByBasisForTransferId(int basisForTransferId);
        IQueryable<DataProcessingRegistration> GetByDataResponsibleId(int dataResponsibleId);
        IQueryable<DataProcessingRegistration> GetByOversightOptionId(int oversightOptionId);
        IQueryable<DataProcessingRegistration> GetByContractId(int sourceId);
        IQueryable<DataProcessingRegistration> AsQueryable();
    }
}
