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

        
        /* imports organization units into an organization */
        public IEnumerable<MoxImportError> Import(Stream stream, int organizationId, User kitosUser)
        {
            var errors = new List<MoxImportError>();

            var set = _excelHandler.Import(stream);

            //importing org units
            var orgTable = set.Tables[0];
            errors.AddRange(ImportOrgUnits(orgTable, organizationId, kitosUser));

            return errors;
        }


        private static long? StringToEAN(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;

            //if the ean was properly entered, excel will treat is as a Number,
            //which is bascially a string in double format i.e "12345678.0"
            //so try to parse as double first
            double dbl;
            if (!double.TryParse(s, out dbl)) return null;

            //then convert to long
            return Convert.ToInt64(dbl);
        }

        private static int? StringToId(string s)
        {
            if (String.IsNullOrEmpty(s)) return null;
            return Convert.ToInt32(Convert.ToDouble(s));
        }

        private IEnumerable<MoxImportError> ImportOrgUnits(DataTable orgTable, int organizationId, User kitosUser)
        {
            var errors = new List<MoxImportError>();

            //resolvedRows are the orgUnits that already has been added to the DB.
            //the key is the name of the orgUnit;
            var resolvedRows = new Dictionary<string, OrgUnitRow>();

            //unresolved rows are orgUnits which still needs to be added to the DB.
            var unresolvedRows = new List<OrgUnitRow>();

            //preliminary pass and error checking
            //split the rows into the old org units (already in db)
            //and the new rows that the users has added to the sheet
            var rowIndex = 2;
            foreach(var row in orgTable.AsEnumerable())
            {
                //a row is new if the first column, the id, is empty
                var id = StringToId(row.Field<string>(0));
                var isNew = (id == null);

                var orgUnitRow = new OrgUnitRow()
                {
                    RowIndex = rowIndex, //needed for error reporting
                    IsNew = isNew,
                    Id = id,
                    Name = row.Field<string>(1),
                    Ean = StringToEAN(row.Field<string>(2)),
                    Parent = row.Field<string>(3)
                };

                rowIndex++;
                
                //error checking
                //name cannot be empty
                if (String.IsNullOrWhiteSpace(orgUnitRow.Name))
                {
                    errors.Add(new MoxImportOrgUnitNoNameError(orgUnitRow.RowIndex));
                    continue;
                }
                //parent cannot be empty on a new row
                else if (orgUnitRow.IsNew && String.IsNullOrWhiteSpace(orgUnitRow.Parent))
                {
                    errors.Add(new MoxImportOrgUnitBadParentError(orgUnitRow.RowIndex));
                    continue;
                }
                
                if(isNew) unresolvedRows.Add(orgUnitRow);
                else resolvedRows.Add(orgUnitRow.Name, orgUnitRow);

            }

            //do the actually passes, trying to resolve parents
            var oneMorePass = true;
            while (oneMorePass)
            {
                oneMorePass = false;

                var notResolvedInThisPass = new List<OrgUnitRow>();
                var resolvedInThisPass = new List<OrgUnitRow>();

                foreach (var orgUnitRow in unresolvedRows)
                {
                    //try to locate a parent
                    OrgUnitRow parent;
                    if (resolvedRows.TryGetValue(orgUnitRow.Parent, out parent))
                    {

                        //since a parent was found, insert the new org unit in the DB.
                        var orgUnitEntity = _orgUnitRepository.Insert(new OrganizationUnit
                        {
                            Name = orgUnitRow.Name,
                            Ean = orgUnitRow.Ean,
                            ParentId = parent.Id,
                            ObjectOwnerId = kitosUser.Id,
                            LastChangedByUserId = kitosUser.Id,
                            LastChanged = DateTime.Now,
                            OrganizationId = organizationId
                        });

                        orgUnitRow.Proxy = orgUnitEntity;
                        resolvedInThisPass.Add(orgUnitRow);
                    }
                    else
                    {
                        notResolvedInThisPass.Add(orgUnitRow);
                    }
                }

                if (resolvedInThisPass.Any())
                {
                    oneMorePass = true;
                    //save repository - so the ids of the newly added org units are resolved
                    _orgUnitRepository.Save();

                    foreach (var orgUnitRow in resolvedInThisPass)
                    {
                        orgUnitRow.Id = orgUnitRow.Proxy.Id;
                        resolvedRows.Add(orgUnitRow.Name, orgUnitRow);
                    }
                }

                //if there's still some unresolve rows left, try again
                if (notResolvedInThisPass.Any())
                {
                    unresolvedRows = notResolvedInThisPass;
                }
            }

            //at this point, if there's is any unresolvedRows, we should report some errors
            foreach (var orgUnitRow in unresolvedRows)
            {
                errors.Add(new MoxImportOrgUnitBadParentError(orgUnitRow.RowIndex));
            }
            
            return errors;
        }

        public class MoxImportOrgUnitBadParentError : MoxImportError
        {
            public MoxImportOrgUnitBadParentError(int row)
            {
                Row = row;
                Column = "D";
                SheetName = "Organisation";
                Message = "Overordnet enhed må ikke være blank og skal henvise til anden enhed.";
            }
        }

        public class MoxImportOrgUnitNoNameError : MoxImportError
        {
            public MoxImportOrgUnitNoNameError(int row)
            {
                Row = row;
                Column = "B";
                SheetName = "Organisation";
                Message = "Enheden skal have et navn.";
            }
        }
            
        private class OrgUnitRow
        {
            public int RowIndex { get; set; }
            public bool IsNew { get; set; }
            public int? Id { get; set; }
            public string Name { get; set; }
            public long? Ean { get; set; }
            public string Parent { get; set; }
            public OrganizationUnit Proxy { get; set; }
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
