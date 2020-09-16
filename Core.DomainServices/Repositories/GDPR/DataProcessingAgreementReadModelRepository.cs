﻿using System;
using System.Linq;
using Core.DomainModel.GDPR.Read;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.GDPR
{
    public class DataProcessingAgreementReadModelRepository : IDataProcessingAgreementReadModelRepository
    {
        private readonly IGenericRepository<DataProcessingAgreementReadModel> _repository;

        public DataProcessingAgreementReadModelRepository(IGenericRepository<DataProcessingAgreementReadModel> repository)
        {
            _repository = repository;
        }

        public DataProcessingAgreementReadModel Add(DataProcessingAgreementReadModel newModel)
        {
            var existing = GetBySourceId(newModel.SourceEntityId);

            if (existing.HasValue)
                throw new InvalidOperationException("Only one read model per entity is allowed");

            var inserted = _repository.Insert(newModel);
            _repository.Save();
            return inserted;
        }

        public Maybe<DataProcessingAgreementReadModel> GetBySourceId(int sourceId)
        {
            return _repository.AsQueryable().FirstOrDefault(x => x.SourceEntityId == sourceId);
        }

        public void Update(DataProcessingAgreementReadModel updatedModel)
        {
            _repository.Save();
        }

        public void DeleteBySourceId(int sourceId)
        {
            var readModel = GetBySourceId(sourceId);
            if (readModel.HasValue)
            {
                Delete(readModel.Value);
            }
        }

        public IQueryable<DataProcessingAgreementReadModel> GetByOrganizationId(int organizationId)
        {
            return _repository.AsQueryable().ByOrganizationId(organizationId);
        }

        public void Delete(DataProcessingAgreementReadModel readModel)
        {
            if (readModel == null) throw new ArgumentNullException(nameof(readModel));

            _repository.DeleteWithReferencePreload(readModel);
            _repository.Save();
        }

        public IQueryable<DataProcessingAgreementReadModel> GetByUserId(int userId)
        {
            //Gets all read models that have dependencies on the user
            return _repository
                .AsQueryable()
                .Where(x => x.RoleAssignments.Any(assignment => assignment.UserId == userId))
                .Distinct();
        }
    }
}
