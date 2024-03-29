﻿using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Model;


namespace Core.DomainServices.Repositories.System
{
    public interface IItSystemRepository
    {
        IQueryable<ItSystem> GetSystems(OrganizationDataQueryParameters parameters = null);

        IQueryable<ItSystem> GetUnusedSystems(OrganizationDataQueryParameters parameters);

        IQueryable<ItSystem> GetSystemsInUse(int organizationId);

        ItSystem GetSystem(int systemId);

        Maybe<ItSystem> GetSystem(Guid systemId);

        void DeleteSystem(ItSystem itSystem);
        IQueryable<ItSystem> GetByRightsHolderId(int sourceId);
        IQueryable<ItSystem> GetByTaskRefId(int taskRefId);
        void Add(ItSystem newSystem);
        void Update(ItSystem itSystem);
    }
}
