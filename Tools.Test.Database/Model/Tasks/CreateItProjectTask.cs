using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices.Factories;
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

            var project = ItProjectFactory.Create(_name, commonOrg.Id, DateTime.Now);

            var entities = new List<IEntity>();
            entities.Add(project);
            entities.Add(project.GoalStatus);
            entities.Add(project.Handover);
            entities.AddRange(project.EconomyYears);
            entities.ForEach(entity =>
            {
                entity.ObjectOwner = globalAdmin;
                entity.LastChangedByUser = globalAdmin;
                entity.LastChanged = DateTime.Now;
            });

            context.ItProjects.Add(project);
            context.SaveChanges();

            return true;
        }

        public override string ToString()
        {
            return $"Task: {GetType().Name}. Name: {_name}.";
        }
    }
}
