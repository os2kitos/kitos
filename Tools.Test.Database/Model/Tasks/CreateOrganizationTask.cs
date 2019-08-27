using System;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateOrganizationTask : DatabaseTask
    {
        private readonly string _orgName;
        private readonly int _organizationType;

        public CreateOrganizationTask(string connectionString, string organizationType, string orgName) : base(connectionString)
        {
            _organizationType = ParseInt(organizationType ?? throw new ArgumentNullException(nameof(organizationType)));
            _orgName = orgName ?? throw new ArgumentNullException(nameof(orgName));
        }

        public override bool Execute()
        {
            using (var context = CreateKitosContext())
            {
                var globalAdmin = context.GetGlobalAdmin();

                var organization = new Organization()
                {
                    Name = _orgName,
                    AccessModifier = AccessModifier.Public,
                    TypeId = _organizationType,
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

        private int ParseInt(string input)
        {
            
            if (!int.TryParse(input, out var output))
            {
                throw new ArgumentException($"{nameof(input)} must be an integer");
            }

            return output;
        }
    }
}
