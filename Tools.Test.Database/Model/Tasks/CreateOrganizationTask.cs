using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    ObjectOwnerId = globalAdmin.Id,
                    AccessModifier = AccessModifier.Public,
                    LastChangedByUserId = globalAdmin.Id,
                    TypeId = 1
                };

                context.Organizations.Add(organization);
                context.SaveChanges();
            }

            return true;
        }
    }
}
