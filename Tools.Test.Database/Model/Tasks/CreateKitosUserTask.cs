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
        private readonly string _salt;

        public CreateKitosUserTask(string connectionString, string email, string password, string role)
            : base(connectionString)
        {
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _password = password ?? throw new ArgumentNullException(nameof(password)); ;
            _role = ParseRole(role ?? throw new ArgumentNullException(nameof(role)));
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
                var commonOrg = context.GetCommonOrganization();

                var newUser = CreateUser(commonOrg, context);

                AssignOrganizationRole(context, newUser, commonOrg);

                context.SaveChanges();
            }

            return true;
        }

        private User CreateUser(Organization commonOrg, KitosContext context)
        {
            var globalAdmin = context.GetGlobalAdmin();

            var newUser = new User
            {
                Name = "Automatisk oprettet testbruger",
                LastName = $"({_role:G})",
                Salt = _salt,
                Email = _email,
                DefaultOrganizationId = commonOrg.Id,
                ObjectOwnerId = globalAdmin.Id,
                LastChangedByUserId = globalAdmin.Id,
                IsGlobalAdmin = _role == OrganizationRole.GlobalAdmin
            };
            newUser.SetPassword(_password);

            context.Users.AddOrUpdate(x => x.Email, newUser);
            context.SaveChanges();
            return newUser;
        }

        private void AssignOrganizationRole(KitosContext context, User newUser, Organization commonOrg)
        {
            var globalAdmin = context.GetGlobalAdmin();
            var newRight = new OrganizationRight
            {
                UserId = newUser.Id,
                Role = _role,
                OrganizationId = commonOrg.Id,
                ObjectOwnerId = globalAdmin.Id,
                LastChangedByUserId = globalAdmin.Id
            };

            context.OrganizationRights.AddOrUpdate(x => x.UserId, newRight);
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
    }
}
