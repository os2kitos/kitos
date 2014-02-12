using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web;
using System.Web.Security;
using Core.DomainServices;
using Ninject;

namespace UI.MVC4.Infrastructure
{
    public class CustomRoleProvider : RoleProvider
    {
        [Inject]
        public IUserRepository UserRepository { get; set; }

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
            if(String.IsNullOrEmpty(username))
                throw new ProviderException("Bad username: null or empty");

            var user = UserRepository.GetByUsername(username);
            if (user == null)
            {
                return new string[]{};
            } 

            return user.Roles.Select(r => r.Name).ToArray();
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
            var user = UserRepository.GetByUsername(username);

            if (user == null) return false;

            return user.Roles.Any(role => role.Name == roleName);
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