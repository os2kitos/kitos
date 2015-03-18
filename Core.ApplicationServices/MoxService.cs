using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Transactions;
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
        private readonly IGenericRepository<AdminRight> _adminRightRepository;
        private readonly IExcelHandler _excelHandler;

        public MoxService(IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<OrganizationRole> orgRoleRepository,
            IGenericRepository<TaskRef> taskRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<AdminRight> adminRightRepository,
            IExcelHandler excelHandler)
        {
            _orgUnitRepository = orgUnitRepository;
            _orgRoleRepository = orgRoleRepository;
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _adminRightRepository = adminRightRepository;
            _excelHandler = excelHandler;
        }

        // Export Users
        public Stream ExportUsers(Stream stream, int organizationId, User kitosUSer)
        {
            var users = _userRepository.Get(x => x.AdminRights.Count(r => r.ObjectId == organizationId) > 0);

            var set = new DataSet();
            set.Tables.Add(GetUserTable(users));

            return _excelHandler.Export(set, stream);
        }

        /* Export Organizations */
        public Stream Export(Stream stream, int organizationId, User kitosUser)
        {
            var orgUnits = _orgUnitRepository.Get(x => x.OrganizationId == organizationId).ToList();
            //dynamic orgRoles = null;
            //dynamic orgTasks = null;
            //foreach (var orgUnit in orgUnits)
            //{
            //    var unitName = orgUnit.Name;
            //    orgRoles = orgUnit.Rights.Select(x => new {OrgUnit = unitName, Role = x.Role.Name, User = x.User.Name});
            //    orgTasks =
            //        orgUnit.TaskUsages.Select(x => new { OrgUnit = unitName, Task = x.TaskRefId, Overview = x.Starred });
            //}

            //var roles = _orgRoleRepository.Get(x => x.IsActive && !x.IsSuggestion);
            //var tasks = _taskRepository.Get(x => x.AccessModifier == AccessModifier.Public);
            //var users = _userRepository.Get(x => x.CreatedInId == organizationId);

            var set = new DataSet();
            set.Tables.Add(GetOrganizationTable(orgUnits));
            //set.Tables.Add(GetOrgRoleTable(orgRoles));
            //set.Tables.Add(GetOrgTaskTable(orgTasks));
            //set.Tables.Add(GetRoleTable(roles));
            //set.Tables.Add(GetTaskTable(tasks));
            //set.Tables.Add(GetUserTable(users));

            return _excelHandler.Export(set, stream);
        }

        public void ImportUsers(Stream stream, int organizationId, User kitosUser)
        {
            var scope = new TransactionScope(TransactionScopeOption.RequiresNew);
            using (scope)
            {
                var errors = new List<MoxImportError>();

                var set = _excelHandler.Import(stream);

                //importing org units
                var userTable = set.Tables[5];
                errors.AddRange(ImportUsersTransaction(userTable, organizationId, kitosUser));

                //how to import something else
                //var anotherTable = set.Tables[1];
                //errors.AddRange(ImportFooBar(anotherTable, foo, bar)

                //then finally, did we notice any errors?
                if (errors.Any()) throw new MoxImportException() { Errors = errors };

                //if we got here, we're home frreeeee
                scope.Complete();

            }
        }

        private IEnumerable<MoxImportError> ImportUsersTransaction(DataTable userTable, int organizationId,
            User kitosUser)
        {
            var errors = new List<MoxImportError>();

            //resolvedRows are the orgUnits that already has been added to the DB.
            //the key is the name of the orgUnit;
            var resolvedRows = new Dictionary<string, UserRow>();

            //unresolved rows are orgUnits which still needs to be added to the DB.
            var unresolvedRows = new List<UserRow>();

            //preliminary pass and error checking
            //split the rows into the old org units (already in db)
            //and the new rows that the users has added to the sheet
            var rowIndex = 2;
            foreach (var row in userTable.AsEnumerable())
            {
                //a row is new if the first column, the id, is empty
                var id = StringToId(row.Field<string>(1));
                var isNew = (id == null);

                var userRow = new UserRow()
                {
                    RowIndex = rowIndex, //needed for error reporting
                    IsNew = isNew,
                    Id = id,
                    Name = row.Field<string>(2),
                    LastName = row.Field<string>(3),
                    Email = row.Field<string>(4),
                    Phone = row.Field<string>(5)
                };

                rowIndex++;

                //error checking
                //name cannot be empty
                if (String.IsNullOrWhiteSpace(userRow.Name))
                {
                    var error = new MoxImportError()                    
                    {
                        Row = userRow.RowIndex,
                        Column = "C",
                        Message = "Fornavn mangler",
                        SheetName = "Brugere"
                    };

                    errors.Add(error);
                }
                else if (String.IsNullOrWhiteSpace(userRow.LastName))
                {
                    var error = new MoxImportError()
                    {
                        Row = userRow.RowIndex,
                        Column = "D",
                        Message = "Efternavn(e) mangler",
                        SheetName = "Brugere"
                    };

                    errors.Add(error);
                }
                //email cannot be empty
                else if (String.IsNullOrWhiteSpace(userRow.Email))
                {
                    var error = new MoxImportError()
                    {
                        Row = userRow.RowIndex,
                        Column = "E",
                        Message = "Email mangler",
                        SheetName = "Brugere"
                    };

                    errors.Add(error);
                }

                //otherwise we're good - add the row to either resolved or unresolved
                else if (isNew)
                {
                    unresolvedRows.Add(userRow);
                }
                else
                {
                    resolvedRows.Add(userRow.Name, userRow);
                }

            }

            //do the actually passes, trying to resolve parents
            var oneMorePass = true;
            while (oneMorePass && unresolvedRows.Any())
            {
                oneMorePass = false;

                var notResolvedInThisPass = new List<UserRow>();
                var resolvedInThisPass = new List<UserRow>();

                foreach (var userRow in unresolvedRows)
                {
                    var userEntity = new User()
                    {
                        Name = userRow.Name,
                        LastName = userRow.LastName,
                        Email = userRow.Email,
                        PhoneNumber = userRow.Phone,
                        ObjectOwnerId = kitosUser.Id,
                        LastChangedByUserId = kitosUser.Id,
                        LastChanged = DateTime.Now,
                        IsGlobalAdmin = false,
                        Password = "mangler at blive indsat",
                        Salt = "mangler at blive indsat"
                    };

                    //If user dosnt exist create a new one.
                    if (!_userRepository.Get(x => x.Email == userEntity.Email).Any())
                    {
                        _userRepository.Insert(userEntity);
                        _userRepository.Save();
                    }
                    else
                    {
                        //Get user to ensure we're using the correct information
                        userEntity = _userRepository.Get(x => x.Email == userEntity.Email).First();
                    }

                    resolvedInThisPass.Add(userRow);

                    //If adminRight exists, no further action is needed
                    if(_adminRightRepository.Get(x => x.User.Email == userEntity.Email).Any())
                        continue;

                    //Create the adminright within the organization
                    _adminRightRepository.Insert(new AdminRight()
                    {
                        ObjectId = organizationId,
                        UserId = userEntity.Id,
                        RoleId = 2,
                        LastChangedByUserId = kitosUser.Id,
                        LastChanged = DateTime.Now,
                        ObjectOwnerId = kitosUser.Id
                    });
                    _adminRightRepository.Save();

                    
                }
            }

            //at this point, if there's is any unresolvedRows, we should report some errors
            foreach (var orgUnitRow in unresolvedRows)
            {
                //TODO: Implement user-errors
               
            }

            return errors;
        }
        
        /* imports organization units into an organization */
        public void Import(Stream stream, int organizationId, User kitosUser)
        {
            var scope = new TransactionScope(TransactionScopeOption.RequiresNew);
            using (scope)
            {
                var errors = new List<MoxImportError>();

                var set = _excelHandler.Import(stream);

                //importing org units
                var orgTable = set.Tables[0];
                errors.AddRange(ImportOrgUnits(orgTable, organizationId, kitosUser));

                //how to import something else
                //var anotherTable = set.Tables[1];
                //errors.AddRange(ImportFooBar(anotherTable, foo, bar)

                //then finally, did we notice any errors?
                if (errors.Any()) throw new MoxImportException() {Errors = errors};

                //if we got here, we're home frreeeee
                scope.Complete();

            }

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
                }
                //name cannot be duplicate
                else if (unresolvedRows.Any(x => x.Name == orgUnitRow.Name) || resolvedRows.ContainsKey(orgUnitRow.Name))
                {
                    errors.Add(new MoxImportOrgUnitDuplicateError(orgUnitRow.RowIndex));
                }
                //parent cannot be empty on a new row
                else if (orgUnitRow.IsNew && String.IsNullOrWhiteSpace(orgUnitRow.Parent))
                {
                    errors.Add(new MoxImportOrgUnitBadParentError(orgUnitRow.RowIndex));
                } 

                //otherwise we're good - add the row to either resolved or unresolved
                else if (isNew)
                {
                    unresolvedRows.Add(orgUnitRow);
                }
                else
                {
                    resolvedRows.Add(orgUnitRow.Name, orgUnitRow);
                }

            }

            //do the actually passes, trying to resolve parents
            var oneMorePass = true;
            while (oneMorePass && unresolvedRows.Any())
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
                unresolvedRows = notResolvedInThisPass;
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
                Message = "Overordnet enhed må ikke være blank og skal henvise til gyldig enhed.";
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

        public class MoxImportOrgUnitDuplicateError : MoxImportError
        {
            public MoxImportOrgUnitDuplicateError(int row)
            {
                Row = row;
                Column = "B";
                SheetName = "Organisation";
                Message = "Der findes allerede en enhed med dette navn.";
            }
        }

        private class UserRow
        {
            public int RowIndex { get; set; }
            public bool IsNew { get; set; }
            public int? Id { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
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

                table.Rows.Add(lookupString, user.Id, user.Name, user.LastName, user.Email, user.PhoneNumber);
            }

            return table;
        }

        #endregion
    }
}
