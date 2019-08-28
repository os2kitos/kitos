using System;
using System.Data.Entity.Migrations;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateKitosUserTask : DatabaseTask
    {
        private readonly string _email;
        private readonly string _password;
        private readonly OrganizationRole _role;
        private readonly bool _apiAccess;
        private readonly string _salt;
        private readonly string[] _organizationNames;

        public CreateKitosUserTask(string connectionString, string email, string password, string role, string organizationNames, bool apiAccess = false)
            : base(connectionString)
        {
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _role = ParseRole(role ?? throw new ArgumentNullException(nameof(role)));
            _organizationNames = ParseOrganizationNames(organizationNames ?? throw new ArgumentNullException(nameof(organizationNames)));
            _apiAccess = apiAccess;
            _salt = string.Format("{0:N}{0:N}", Guid.NewGuid());
        }


        /// <summary>
        /// Replicates the actions carried out by both org-user-create.controller.ts, UsersController::Post and UserService
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            using (var context = CreateKitosContext())
            {
                var firstOrg = context.GetOrganization(_organizationNames[0]);

                var newUser = CreateUser(firstOrg, context);

                foreach (var orgName in _organizationNames)
                {
                    AssignOrganizationRole(context, newUser, orgName);
                }

                context.SaveChanges();
            }

            return true;
        }

        private User CreateUser(Organization commonOrg, KitosContext context)
        {
            var globalAdmin = context.GetGlobalAdmin();
            var apiUser = "Api ";
            var newUser = new User
            {
                Name = "Automatisk oprettet testbruger",
                LastName = $"({((_apiAccess) ? apiUser : "")}{_role:G})",
                Salt = _salt,
                Email = _email,
                DefaultOrganizationId = commonOrg.Id,
                ObjectOwnerId = globalAdmin.Id,
                LastChangedByUserId = globalAdmin.Id,
                IsGlobalAdmin = _role == OrganizationRole.GlobalAdmin,
                HasApiAccess = _apiAccess
            };
            newUser.SetPassword(_password);

            context.Users.AddOrUpdate(x => x.Email, newUser);
            context.SaveChanges();
            return newUser;
        }

        private void AssignOrganizationRole(KitosContext context, User newUser, string orgName)
        {
            var globalAdmin = context.GetGlobalAdmin();
            var org = context.GetOrganization(orgName);
            var newRight = new OrganizationRight
            {
                UserId = newUser.Id,
                Role = _role,
                OrganizationId = org.Id,
                ObjectOwnerId = globalAdmin.Id,
                LastChangedByUserId = globalAdmin.Id
            };

            context.OrganizationRights.Add(newRight);
            context.SaveChanges();
        }

        private OrganizationRole ParseRole(string role)
        {
            var organizationRoles = Enum.GetValues(typeof(OrganizationRole)).Cast<OrganizationRole>().ToDictionary(x => x.ToString("G"), StringComparer.OrdinalIgnoreCase);
            if (!organizationRoles.TryGetValue(role, out var actualRole))
            {
                throw new ArgumentException($"{nameof(role)} must be one of [{string.Join(",", organizationRoles.Keys)}]");
            }

            return actualRole;
        }

        private string[] ParseOrganizationNames(string organizationNames)
        {
            return organizationNames.Split(',');
        }
    }
}
