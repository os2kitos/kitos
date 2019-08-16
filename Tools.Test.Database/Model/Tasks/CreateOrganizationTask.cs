using System;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateOrganizationTask : DatabaseTask
    {
        private readonly string _name;

        public CreateOrganizationTask(string connectionString, string name) : base(connectionString)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override bool Execute()
        {
            using (var context = CreateKitosContext())
            {
                var globalAdmin = context.GetGlobalAdmin();

                var organization = new Organization()
                {
                    Name = _name,
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
