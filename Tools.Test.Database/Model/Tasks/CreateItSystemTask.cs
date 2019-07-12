using System;
using System.Data.Entity;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateItSystemTask : DatabaseTask
    {
        private readonly string _name;

        public CreateItSystemTask(string connectionString, string name)
            : base(connectionString)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override bool Execute()
        {
            using (var context = CreateKitosContext())
            {
                var commonOrg = context.GetCommonOrganization();
                var globalAdmin = context.GetGlobalAdmin();

                var systemWithSameName = context.ItSystems.AsNoTracking().FirstOrDefault(x => x.Name == _name);
                if (systemWithSameName != null)
                {
                    Console.Out.WriteLine($"Existing ITSystem with name {_name} already exists in the database.");
                    return false;
                }

                var itSystem = new ItSystem()
                {
                    Name = _name,
                    BelongsToId = globalAdmin.Id,
                    OrganizationId = commonOrg.Id,
                    LastChangedByUser = globalAdmin,
                    Uuid = Guid.NewGuid()
                };

                context.ItSystems.Add(itSystem);
                context.SaveChanges();
            }

            return true;
        }
    }
}
