using System;
using System.Linq;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Infrastructure.DataAccess;

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
            _email = email;
            _password = password;
            _role = ParseRole(role);
            _salt = string.Format("{0:N}{0:N}", Guid.NewGuid());
        }

        /// <summary>
        /// Create a user the same way as the steps prescribed in Core.ApplicationServices.UserService.cs::AddUser
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            using (var context = CreateKitosContext())
            {
                var userRepository = new GenericRepository<User>(context);
                var commonOrg = FindCommonOrganization(context);

                var newUser = CreateUser(commonOrg, userRepository);

                AssignOrganizationRole(userRepository, newUser, commonOrg, context);

                context.SaveChanges();
            }
        }

        private static Organization FindCommonOrganization(KitosContext context)
        {
            var orgRepository = new GenericRepository<Organization>(context);
            var commonOrg = orgRepository.AsQueryable().First(x => x.Name == "Fælles Kommune");
            return commonOrg;
        }

        private User CreateUser(Organization commonOrg, GenericRepository<User> userRepository)
        {
            var cryptoService = new CryptoService();

            var newUser = new User
            {
                Name = "Automatisk oprettet testbruger",
                LastName = $"({_role:G})",
                Password = CreatePasswordValue(cryptoService),
                Email = _email,
                DefaultOrganizationId = commonOrg.Id
            };

            userRepository.Insert(newUser);
            userRepository.Save();
            return newUser;
        }

        private void AssignOrganizationRole(GenericRepository<User> userRepository, User newUser, Organization commonOrg,
            KitosContext context)
        {
            var globalAdmin = userRepository.AsQueryable().First(x => x.IsGlobalAdmin);
            var newRight = new OrganizationRight
            {
                UserId = newUser.Id,
                Role = _role,
                OrganizationId = commonOrg.Id,
                ObjectOwnerId = globalAdmin.Id,
                LastChangedByUserId = globalAdmin.Id
            };

            var orgRightRepository = new GenericRepository<OrganizationRight>(context);
            orgRightRepository.Insert(newRight);
            orgRightRepository.Save();
        }

        private string CreatePasswordValue(CryptoService cryptoService)
        {
            return cryptoService.Encrypt(_password + _salt);
        }

        private OrganizationRole ParseRole(string role)
        {
            var organizationRoles = Enum.GetValues(typeof(OrganizationRole)).Cast<OrganizationRole>().ToDictionary(x=>x.ToString("G"),StringComparer.OrdinalIgnoreCase);
            if (!organizationRoles.TryGetValue(role, out var actualRole))
            {
                throw new ArgumentException($"{nameof(role)} must be one of [{string.Join(",", organizationRoles.Keys)}]");
            }
            return actualRole;
        }
    }
}
