using System;
using System.Web.Security;
using Ninject;

namespace Presentation.Web.Infrastructure
{
    public class CustomRoleProvider : RoleProvider
    {
        [Inject]
        public IUserRepositoryFactory UserRepositoryFactory { get; set; }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the names of the roles of a user
        /// </summary>
        /// <param name="username"></param>
        /// <returns>
        /// Array of (the names of) the roles of the user, or empty array
        /// if user doesn't exist or has no roles
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {
            throw new NotImplementedException();

            /*
            var userRepository = UserRepositoryFactoryMock.GetUserRepository();
            if(String.IsNullOrEmpty(username))
                throw new ProviderException("Bad username: null or empty");

            var user = userRepository.GetByEmail(username);
            if (user == null || user.Role == null)
            {
                return new string[]{};
            }

            return new string[] {user.Role.Name};*/
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="username"></param>
        /// <param name="roleName"></param>
        /// <returns>True if and only if user exists and has that role</returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();

            /*
            var userRepository = UserRepositoryFactoryMock.GetUserRepository();
            var user = userRepository.GetByEmail(username);

            if (user == null || user.Role == null) return false;

            return user.Role.Name == roleName;*/
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}
