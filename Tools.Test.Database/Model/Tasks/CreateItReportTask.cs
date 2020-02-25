using System;
using System.Linq;
using Core.DomainModel.Reports;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateItReportTask : DatabaseTask
    {
        private readonly string _name;

        public CreateItReportTask(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override bool Execute(KitosContext context)
        {
            var commonOrg = context.GetOrganization(TestOrganizations.CommonOrg);
            var globalAdmin = context.GetGlobalAdmin();

            var existing = context.Reports.AsNoTracking().FirstOrDefault(x => x.Name == _name);
            if (existing != null)
            {
                Console.Out.WriteLine($"Existing Report with name {_name} already exists in the database.");
                return false;
            }

            var entity = new Report()
            {
                Name = _name,
                ObjectOwnerId = globalAdmin.Id,
                OrganizationId = commonOrg.Id,
                LastChangedByUserId = globalAdmin.Id,
                Description = "Test rapport oprettet til accessibility tests"
            };

            context.Reports.Add(entity);
            context.SaveChanges();

            return true;
        }

        public override string ToString()
        {
            return $"Task: {GetType().Name}. Name: {_name}.";
        }
    }
}
