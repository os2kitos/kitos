using System;
using System.Linq;
using Core.DomainModel.ItSystem;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateItContractTask : DatabaseTask
    {
        private readonly string _name;

        public CreateItContractTask(string connectionString, string name) 
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

                var systemWithSameName = context.ItContracts.AsNoTracking().FirstOrDefault(x => x.Name == _name);
                if (systemWithSameName != null)
                {
                    Console.Out.WriteLine($"Existing ItContract with name {_name} already exists in the database.");
                    return false;
                }

                var itSystem = new ItSystem
                {
                    Name = _name,
                    ObjectOwner = globalAdmin,
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
