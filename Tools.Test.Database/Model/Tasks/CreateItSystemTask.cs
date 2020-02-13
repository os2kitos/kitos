using System;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateItSystemTask : DatabaseTask
    {
        private readonly string _name;
        private readonly string _organizationName;
        private readonly int? _parentId;

        public CreateItSystemTask(string name, string organizationName, int? parentId)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _organizationName = organizationName;
            _parentId = parentId;
        }

        public override bool Execute(KitosContext context)
        {
            var organization = context.GetOrganization(_organizationName);
            var globalAdmin = context.GetGlobalAdmin();

            //Create the new it system
            var systemWithSameName = GetItSystemnByName(context);
            if (systemWithSameName != null)
            {
                Console.Out.WriteLine($"Existing ITSystem with name {_name} already exists in the database.");
                return false;
            }

            var itSystem = new ItSystem
            {
                Name = _name,
                ObjectOwnerId = globalAdmin.Id,
                OrganizationId = organization.Id,
                LastChangedByUserId = globalAdmin.Id,
                Uuid = Guid.NewGuid(),
                ParentId = _parentId
            };

            context.ItSystems.Add(itSystem);
            context.SaveChanges();

            //Enable usage of the system
            var itSystemUsage = new ItSystemUsage
            {
                ItSystemId = itSystem.Id,
                OrganizationId = organization.Id,
                ObjectOwnerId = globalAdmin.Id,
                LastChangedByUserId = globalAdmin.Id,
                DataLevel = itSystem.DataLevel,
                ContainsLegalInfo = itSystem.ContainsLegalInfo,
                AssociatedDataWorkers = itSystem.AssociatedDataWorkers.Select(x => new ItSystemUsageDataWorkerRelation
                {
                    LastChangedByUserId = x.LastChangedByUserId,
                    ObjectOwnerId = x.ObjectOwnerId,
                    DataWorkerId = x.DataWorkerId
                }).ToList()
            };

            itSystemUsage.ResponsibleUsage = new ItSystemUsageOrgUnitUsage()
            {
                ItSystemUsage = itSystemUsage,
                ItSystemUsageId = itSystemUsage.Id,
                OrganizationUnit = organization.OrgUnits.First(),
                OrganizationUnitId = organization.Id
            };

            context.ItSystemUsages.Add(itSystemUsage);
            context.SaveChanges();

            foreach (var option in context.AttachedOptions.Where(x => x.ObjectId == itSystem.Id && x.ObjectType == EntityType.ITSYSTEM))
            {
                option.ObjectId = itSystemUsage.Id;
                option.ObjectType = EntityType.ITSYSTEMUSAGE;
                context.AttachedOptions.Add(option);
            }

            context.SaveChanges();

            return true;
        }

        private ItSystem GetItSystemnByName(KitosContext context)
        {
            return context.ItSystems.AsNoTracking().FirstOrDefault(x => x.Name == _name);
        }

        public override string ToString()
        {
            var parentId = _parentId?.ToString() ?? "_none_";
            return $"Task: {GetType().Name}. Name: {_name}. Organization: {_organizationName}. ParentId: {parentId}";
        }
    }
}
