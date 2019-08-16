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
        private readonly bool _secondOrganization;

        public CreateItSystemTask(string connectionString, string name, bool secondOrganization)
            : base(connectionString)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _secondOrganization = secondOrganization;
        }

        public override bool Execute()
        {
            using (var context = CreateKitosContext())
            {
                var organization = _secondOrganization ? context.GetSecondOrganization() : context.GetCommonOrganization();
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
                    Uuid = Guid.NewGuid()
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
            }

            return true;
        }

        private ItSystem GetItSystemnByName(KitosContext context)
        {
            return context.ItSystems.AsNoTracking().FirstOrDefault(x => x.Name == _name);
        }
    }
}
