using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class AdminService : IAdminService
    {
        private readonly IGenericRepository<AdminRight> _adminRights;
        private readonly IGenericRepository<AdminRole> _adminRoles;

        public AdminService(IGenericRepository<AdminRight> adminRights, IGenericRepository<AdminRole> adminRoles)
        {
            _adminRights = adminRights;
            _adminRoles = adminRoles;
        }

        public AdminRight MakeLocalAdmin(User user, Organization organization)
        {
            var result = _adminRights.Insert(new AdminRight
                {
                    Object = organization,
                    User = user,
                    Role = GetLocalAdminRole()
                });
            _adminRights.Save();

            return result;
        }

        public void RemoveLocalAdmin(User user, Organization organization)
        {
            var role = GetLocalAdminRole();
            _adminRights.DeleteByKey(organization.Id, role.Id, user.Id);
        }

        public bool IsGlobalAdmin(User user)
        {
            return user.IsGlobalAdmin;
        }

        public bool IsLocalAdmin(User user, Organization organization)
        {
            return user.AdminRights.Any(right => right.Role.Name == "LocalAdmin" && right.ObjectId == organization.Id);
        }

        public AdminRole GetLocalAdminRole()
        {
            return _adminRoles.Get(role => role.Name == "LocalAdmin").First();
        }

        public IEnumerable<AdminRight> GetAdminRights()
        {
            return _adminRights.Get();
        }
    }
}