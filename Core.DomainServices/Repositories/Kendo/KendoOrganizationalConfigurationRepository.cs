﻿using Core.DomainModel;
using Infrastructure.Services.Types;
using System.Linq;

namespace Core.DomainServices.Repositories.Kendo
{
    public class KendoOrganizationalConfigurationRepository : IKendoOrganizationalConfigurationRepository
    {
        private readonly IGenericRepository<KendoOrganizationalConfiguration> _repository;

        public KendoOrganizationalConfigurationRepository(IGenericRepository<KendoOrganizationalConfiguration> repository)
        {
            _repository = repository;
        }

        public KendoOrganizationalConfiguration Add(KendoOrganizationalConfiguration createdConfig)
        {
            var config = _repository.Insert(createdConfig);
            _repository.Save();
            return config;
        }

        public Maybe<KendoOrganizationalConfiguration> Get(int organizationId, OverviewType overviewType)
        {
            return _repository.AsQueryable().Where(x => x.OrganizationId == organizationId && x.OverviewType == overviewType).SingleOrDefault();
        }

        public void Delete(KendoOrganizationalConfiguration configToDelete)
        {
            _repository.Delete(configToDelete);
            _repository.Save();
        }

        public void Update(KendoOrganizationalConfiguration modifiedConfig)
        {
            _repository.Update(modifiedConfig);
            _repository.Save();
        }
    }
}
