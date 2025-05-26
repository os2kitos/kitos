using System;
using System.Data.Entity.Migrations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;
using Tools.Test.Database.Model.Parameters;

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

        public CreateKitosUserTask(string email, string password, OrganizationRole role, string organizationNames, bool apiAccess = false)
        {
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _role = role;
            _organizationNames = ParseOrganizationNames(organizationNames ?? throw new ArgumentNullException(nameof(organizationNames)));
            _apiAccess = apiAccess;
            _salt = string.Format("{0:N}{0:N}", Guid.NewGuid());
        }

        public CreateKitosUserTask(Credentials credentials, OrganizationRole role, string organizationNames, bool apiAccess = false)
        : this(credentials.Email, credentials.Password, role, organizationNames, apiAccess)
        {

        }


        /// <summary>
        /// Replicates the actions carried out by both org-user-create.controller.ts, UsersController::Post and UserService
        /// </summary>
        /// <returns></returns>
        public override bool Execute(KitosContext context)
        {
            var newUser = CreateUser(context);

            foreach (var orgName in _organizationNames)
            {
                AssignOrganizationRole(context, newUser, orgName);
            }

            context.SaveChanges();

            return true;
        }

        private User CreateUser(KitosContext context)
        {
            var globalAdmin = context.GetGlobalAdmin();
            var apiUser = "Api ";
            var newUser = new User
            {
                Name = "Automatisk oprettet testbruger",
                LastName = $"({((_apiAccess) ? apiUser : "")}{_role:G})",
                Salt = _salt,
                Email = _email,
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
                Role = _role == OrganizationRole.GlobalAdmin ? OrganizationRole.User : _role,
                OrganizationId = org.Id,
                ObjectOwnerId = globalAdmin.Id,
                LastChangedByUserId = globalAdmin.Id
            };

            context.OrganizationRights.Add(newRight);
            context.SaveChanges();
        }

        private string[] ParseOrganizationNames(string organizationNames)
        {
            return organizationNames.Split(',');
        }

        public override string ToString()
        {
            return $"Task: {GetType().Name}. Role:{_role:G}. Email:{_email}. API Access:{_apiAccess}. Organizations: {string.Join(",", _organizationNames)}";
        }
    }
}
