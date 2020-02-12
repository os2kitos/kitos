using System;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateTaskRefTask : DatabaseTask
    {
        private readonly string _organizationName;

        public CreateTaskRefTask(string organizationName)
        {
            _organizationName = organizationName;
        }

        public override bool Execute(KitosContext context)
        {
            var organization = context.GetOrganization(_organizationName);
            var globalAdmin = context.GetGlobalAdmin();

            var taskRef = new TaskRef()
            {
                Uuid = new Guid(),
                Type = "TestKLEType",
                TaskKey = "TestKLEKey",
                Description = "Test task ref",
                OwnedByOrganizationUnitId = organization.Id,
                ObjectOwnerId = globalAdmin.Id,
                LastChanged = DateTime.Now,
                LastChangedByUserId = globalAdmin.Id
            };

            context.TaskRefs.Add(taskRef);
            context.SaveChanges();

            return true;
        }

        public override string ToString()
        {
            return $"Task: {GetType().Name}. OrgName: {_organizationName}.";
        }
    }
}
