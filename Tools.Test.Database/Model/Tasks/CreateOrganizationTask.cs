using System;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateOrganizationTask : DatabaseTask
    {

        public CreateOrganizationTask(string connectionString) : base(connectionString)
        {
        }

        public override bool Execute()
        {
            using (var context = CreateKitosContext())
            {
                var globalAdmin = context.GetGlobalAdmin();

                var organization = new Organization()
                {
                    Name = TestOrganizations.secondTestOrg,
                    AccessModifier = AccessModifier.Public,
                    TypeId = 1,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id
                };

                organization.Config = Config.Default(globalAdmin);
                organization.OrgUnits.Add(new OrganizationUnit()
                {
                    Name = organization.Name,
                    ObjectOwnerId = globalAdmin.Id,
                    LastChangedByUserId = globalAdmin.Id
                });

                context.Organizations.Add(organization);
                context.SaveChanges();
            }

            return true;
        }
    }
}
