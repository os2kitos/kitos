using System;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateTaskRefTask : DatabaseTask
    {
        private string _organizationName;

        public CreateTaskRefTask(string connectionString, string organizationName) : base(connectionString)
        {
            _organizationName = organizationName;
        }

        public override bool Execute()
        {
            using (var context = CreateKitosContext())
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
            }

            return true;
        }
    }
}
