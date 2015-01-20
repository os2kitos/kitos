using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public interface IMoxService
    {
        Stream Export(Stream stream, int organizationId, User kitosUser);
        void Import(Stream stream);
    }

    public class MoxService : IMoxService
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<OrganizationRole> _orgRoleRepository;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IMox _mox;

        public MoxService(IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<OrganizationRole> orgRoleRepository,
            IGenericRepository<TaskRef> taskRepository,
            IGenericRepository<User> userRepository,
            IMox mox)
        {
            _orgUnitRepository = orgUnitRepository;
            _orgRoleRepository = orgRoleRepository;
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _mox = mox;
        }

        public Stream Export(Stream stream, int organizationId, User kitosUser)
        {
            var orgUnits = _orgUnitRepository.Get(x => x.OrganizationId == organizationId).ToList();
            dynamic orgRoles = null;
            dynamic orgTasks = null;
            foreach (var orgUnit in orgUnits)
            {
                var unitName = orgUnit.Name;
                orgRoles = orgUnit.Rights.Select(x => new {OrgUnit = unitName, Role = x.Role.Name, User = x.User.Name});
                orgTasks =
                    orgUnit.TaskUsages.Select(x => new { OrgUnit = unitName, Task = x.TaskRefId, Overview = x.Starred });
            }

            var roles = _orgRoleRepository.Get(x => x.IsActive && !x.IsSuggestion);
            var tasks = _taskRepository.Get(x => x.AccessModifier == AccessModifier.Public);
            var users = _userRepository.Get(x => x.CreatedInId == organizationId);

            var set = new DataSet();
            set.Tables.Add(GetOrganizationTable(orgUnits));
            set.Tables.Add(GetOrgRoleTable(orgRoles));
            set.Tables.Add(GetOrgTaskTable(orgTasks));
            set.Tables.Add(GetRoleTable(roles));
            set.Tables.Add(GetTaskTable(tasks));
            set.Tables.Add(GetUserTable(users));

            return _mox.Export(set, stream);
        }

        public void Import(Stream stream)
        {
            
        }

        #region Table Helpers

        private static DataTable GetOrganizationTable(IEnumerable<OrganizationUnit> orgUnits)
        {
            var table = new DataTable("Organisation");
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();

            foreach (var orgUnit in orgUnits)
            {
                var parent = "";
                if (orgUnit.Parent != null)
                    parent = orgUnit.Parent.Name;
                
                table.Rows.Add("", orgUnit.Id, orgUnit.Name, orgUnit.Ean, parent);
            }

            return table;
        }

        private static DataTable GetOrgRoleTable(IEnumerable<dynamic> orgRoles)
        {
            var table = new DataTable("Organisationsrolle");
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();

            foreach (var orgRole in orgRoles)
            {
                table.Rows.Add("", orgRole.OrgUnit, orgRole.Role, orgRole.User);
            }

            return table;
        }

        private static DataTable GetOrgTaskTable(IEnumerable<dynamic> orgRoles)
        {
            var table = new DataTable("Organisationsopgaver");
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();

            foreach (var orgRole in orgRoles)
            {
                table.Rows.Add("", orgRole.OrgUnit, orgRole.Task, orgRole.Overview);
            }

            return table;
        }

        private static DataTable GetRoleTable(IEnumerable<OrganizationRole> roles)
        {
            var table = new DataTable("Rolle");
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();

            foreach (var role in roles)
            {
                table.Rows.Add("", role.Name, role.Id);
            }

            return table;
        }

        private static DataTable GetTaskTable(IEnumerable<TaskRef> tasks)
        {
            var table = new DataTable("Opgave");
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();

            foreach (var task in tasks)
            {
                var lookupString = task.TaskKey + " " + task.Description;
                table.Rows.Add("", lookupString, task.Id, task.TaskKey, task.Description);
            }

            return table;
        }

        private static DataTable GetUserTable(IEnumerable<User> users)
        {
            var table = new DataTable("Bruger");
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();

            foreach (var user in users)
            {
                var lookupString = user.Name + " " + user.Email;
                var defaultOrgUnitName = "";
                if (user.DefaultOrganizationUnit != null)
                    defaultOrgUnitName = user.DefaultOrganizationUnit.Name;

                table.Rows.Add("", lookupString, user.Id, user.Name, user.Email, user.DefaultOrganizationUnitId, defaultOrgUnitName);
            }

            return table;
        }

        #endregion
    }
}
