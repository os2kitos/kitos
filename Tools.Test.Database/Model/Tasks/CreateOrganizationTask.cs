using System;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateOrganizationTask : DatabaseTask
    {
        private readonly string _orgName;
        private readonly int _organizationType;

        public CreateOrganizationTask(int organizationType, string orgName)
        {
            _organizationType = organizationType;
            _orgName = orgName ?? throw new ArgumentNullException(nameof(orgName));
        }

        public override bool Execute(KitosContext context)
        {
            var globalAdmin = context.GetGlobalAdmin();

            var organization = new Organization
            {
                Name = _orgName,
                AccessModifier = AccessModifier.Public,
                TypeId = _organizationType,
                ObjectOwnerId = globalAdmin.Id,
                LastChangedByUserId = globalAdmin.Id,
                Config = Config.Default(globalAdmin)
            };

            organization.OrgUnits.Add(new OrganizationUnit()
            {
                Name = organization.Name,
                ObjectOwnerId = globalAdmin.Id,
                LastChangedByUserId = globalAdmin.Id
            });

            context.Organizations.Add(organization);
            context.SaveChanges();

            return true;
        }

        public override string ToString()
        {
            return $"Task: {GetType().Name}. Name: {_orgName}. Type:{((OrganizationTypeKeys)_organizationType):G}";
        }
    }
}
