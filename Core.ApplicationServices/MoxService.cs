using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class MoxService : IMoxService
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<OrganizationRole> _orgRoleRepository;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IExcelHandler _excelHandler;

        public MoxService(IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<OrganizationRole> orgRoleRepository,
            IGenericRepository<TaskRef> taskRepository,
            IGenericRepository<User> userRepository,
            IExcelHandler excelHandler)
        {
            _orgUnitRepository = orgUnitRepository;
            _orgRoleRepository = orgRoleRepository;
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _excelHandler = excelHandler;
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

            return _excelHandler.Export(set, stream);
        }

        private static long? ToNullableLong(string s)
        {
            long i;
            if (long.TryParse(s, out i)) return i;
            return null;
        }
        
        /* imports organization units into an organization */
        public void Import(Stream stream, int organizationId, User kitosUser)
        {
            var set = _excelHandler.Import(stream);
            var orgTable = set.Tables[1];

            // existing orgUnits
            var existingDatarows =
                orgTable.AsEnumerable()
                    .Where(x => !String.IsNullOrEmpty(x.Field<string>(1)));

            var existing = existingDatarows.Select(x => new OrgUnit
            {
                Id = Convert.ToInt32(Convert.ToDouble(x.Field<string>(1))), 
                Name = x.Field<string>(2)
            }).ToList();

            // filter (remove) orgunits without an ID and groupby parent
            var newOrgUnitsGrouped =
                orgTable.AsEnumerable()
                    .Where(x => String.IsNullOrEmpty(x.Field<string>(1)))
                    .Select(x => new OrgUnit { Name = x.Field<string>(2), Parent = x.Field<string>(4), Ean = ToNullableLong(x.Field<string>(3)) })
                    .GroupBy(x => x.Parent).ToList();

            var count = newOrgUnitsGrouped.Count();
            for (var i = 0; i < count; i++)
            {
                var current = newOrgUnitsGrouped[i];
                // if parentless (root) or parent already exists
                var existingParent = existing.SingleOrDefault(x => x.Name == current.Key);
                if (string.IsNullOrEmpty(current.Key) || existingParent != null)
                {
                    var proxyOrgUnits = new List<OrganizationUnit>();
                    foreach (var orgUnit in current)
                    {
                        var orgUnitEntity = _orgUnitRepository.Insert(new OrganizationUnit
                        {
                            Name = orgUnit.Name,
                            Ean = orgUnit.Ean,
                            ParentId = existingParent == null ? null : (int?)existingParent.Id,
                            ObjectOwnerId = kitosUser.Id,
                            LastChangedByUserId = kitosUser.Id,
                            LastChanged = DateTime.Now,
                            OrganizationId = organizationId
                        });
                        proxyOrgUnits.Add(orgUnitEntity);
                        existing.Add(orgUnit);
                    }
                    _orgUnitRepository.Save();

                    foreach (var orgUnit in current)
                    {
                        var foundProxy = proxyOrgUnits.Single(x => x.Name == orgUnit.Name && x.Ean == orgUnit.Ean);
                        orgUnit.Id = foundProxy.Id;
                    }
                }
                else
                {
                    // else add to end of list, to try and add it after parent have been added
                    newOrgUnitsGrouped.Add(current);
                    count++;
                }
            }
        }
            
        private class OrgUnit
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public long? Ean { get; set; }
            public string Parent { get; set; }
        }

        #region Table Helpers

        private static DataTable GetOrganizationTable(IEnumerable<OrganizationUnit> orgUnits)
        {
            var table = new DataTable("Organisation");
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();

            foreach (var orgUnit in orgUnits)
            {
                var parent = "";
                if (orgUnit.Parent != null)
                    parent = orgUnit.Parent.Name;
                
                table.Rows.Add(orgUnit.Id, orgUnit.Name, orgUnit.Ean, parent);
            }

            return table;
        }

        private static DataTable GetOrgRoleTable(IEnumerable<dynamic> orgRoles)
        {
            var table = new DataTable("Organisationsrolle");
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();

            foreach (var orgRole in orgRoles)
            {
                table.Rows.Add(orgRole.OrgUnit, orgRole.Role, orgRole.User);
            }

            return table;
        }

        private static DataTable GetOrgTaskTable(IEnumerable<dynamic> orgRoles)
        {
            var table = new DataTable("Organisationsopgaver");
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();

            foreach (var orgRole in orgRoles)
            {
                table.Rows.Add(orgRole.OrgUnit, orgRole.Task, orgRole.Overview);
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
                table.Rows.Add(role.Name, role.Id);
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

            foreach (var task in tasks)
            {
                var lookupString = task.TaskKey + " " + task.Description;
                table.Rows.Add(lookupString, task.Id, task.TaskKey, task.Description);
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

            foreach (var user in users)
            {
                var lookupString = user.Name + " " + user.Email;
                var defaultOrgUnitName = "";
                if (user.DefaultOrganizationUnit != null)
                    defaultOrgUnitName = user.DefaultOrganizationUnit.Name;

                table.Rows.Add(lookupString, user.Id, user.Name, user.Email, user.DefaultOrganizationUnitId, defaultOrgUnitName);
            }

            return table;
        }

        #endregion
    }
}
