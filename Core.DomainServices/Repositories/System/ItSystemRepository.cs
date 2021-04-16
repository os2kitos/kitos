﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;

namespace Core.DomainServices.Repositories.System
{
    public class ItSystemRepository : IItSystemRepository
    {
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly IGenericRepository<ItSystem> _systemRepository;

        public ItSystemRepository(
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            IGenericRepository<ItSystem> systemRepository)
        {
            _systemUsageRepository = systemUsageRepository;
            _systemRepository = systemRepository;
        }

        public IQueryable<ItSystem> GetSystems(OrganizationDataQueryParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return _systemRepository
                .AsQueryable()
                .ByOrganizationDataQueryParameters(parameters);
        }

        public IQueryable<ItSystem> GetUnusedSystems(OrganizationDataQueryParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var idsOfSystemsInUse = GetIdsOfSystemsInUse(parameters.ActiveOrganizationId);

            return GetSystems(parameters).ExceptEntitiesWithIds(idsOfSystemsInUse);
        }

        public IQueryable<ItSystem> GetSystemsInUse(int organizationId)
        {
            var idsOfSystemsInUse = GetIdsOfSystemsInUse(organizationId);

            return _systemRepository.AsQueryable().ByIds(idsOfSystemsInUse);
        }

        public ItSystem GetSystem(int systemId)
        {
            return _systemRepository.AsQueryable().ById(systemId);
        }

        public void DeleteSystem(ItSystem itSystem)
        {
            _systemRepository.DeleteWithReferencePreload(itSystem);
            _systemRepository.Save();
        }

        public IQueryable<ItSystem> GetByRightsHolderId(int organizationId)
        {
            return _systemRepository.AsQueryable().Where(x => x.BelongsToId == organizationId);
        }

        public IQueryable<ItSystem> GetByTaskRefId(int taskRefId)
        {
            var systemTaskRefIds = _systemRepository
                .AsQueryable()
                .SelectMany(x => x.TaskRefs.Select(y => new { systemId = x.Id, taskRefId = y.Id}))
                .Where(x => x.taskRefId == taskRefId)
                .Select(x => x.systemId)
                .ToList();
            
            return _systemRepository.AsQueryable().Where(x => systemTaskRefIds.Contains(x.Id));
        }

        private ReadOnlyCollection<int> GetIdsOfSystemsInUse(int organizationId)
        {
            var idsOfSystemsInUse = _systemUsageRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .Select(x => x.ItSystemId)
                .ToList()
                .AsReadOnly();
            return idsOfSystemsInUse;
        }
    }
}
