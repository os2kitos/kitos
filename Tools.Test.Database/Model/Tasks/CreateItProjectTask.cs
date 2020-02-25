using System;
using System.Linq;
using Core.DomainModel.ItProject;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateItProjectTask : DatabaseTask
    {
        private readonly string _name;

        public CreateItProjectTask(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override bool Execute(KitosContext context)
        {
            var commonOrg = context.GetOrganization(TestOrganizations.CommonOrg);
            var globalAdmin = context.GetGlobalAdmin();

            var projectWithName = context.ItProjects.AsNoTracking().FirstOrDefault(x => x.Name == _name);
            if (projectWithName != null)
            {
                Console.Out.WriteLine($"Existing ItProject with name {_name} already exists in the database.");
                return false;
            }

            var itProject = new ItProject
            {
                Name = _name,
                ObjectOwnerId = globalAdmin.Id,
                OrganizationId = commonOrg.Id,
                LastChangedByUserId = globalAdmin.Id,
            };

            context.ItProjects.Add(itProject);
            context.SaveChanges();

            return true;
        }

        public override string ToString()
        {
            return $"Task: {GetType().Name}. Name: {_name}.";
        }
    }
}
