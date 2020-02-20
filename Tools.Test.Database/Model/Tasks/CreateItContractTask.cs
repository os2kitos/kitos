using System;
using System.Linq;
using Core.DomainModel.ItContract;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateItContractTask : DatabaseTask
    {
        private readonly string _name;

        public CreateItContractTask(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override bool Execute(KitosContext context)
        {
            var commonOrg = context.GetOrganization(TestOrganizations.CommonOrg);
            var globalAdmin = context.GetGlobalAdmin();

            var systemWithSameName = context.ItContracts.AsNoTracking().FirstOrDefault(x => x.Name == _name);
            if (systemWithSameName != null)
            {
                Console.Out.WriteLine($"Existing ItContract with name {_name} already exists in the database.");
                return false;
            }

            var itContract = new ItContract
            {
                Name = _name,
                ObjectOwnerId = globalAdmin.Id,
                OrganizationId = commonOrg.Id,
                LastChangedByUserId = globalAdmin.Id,
            };

            context.ItContracts.Add(itContract);
            context.SaveChanges();

            return true;
        }

        public override string ToString()
        {
            return $"Task: {GetType().Name}. Name: {_name}.";
        }
    }
}
