using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Transactions;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Cryptography;

namespace Core.ApplicationServices
{
    public class ExcelService : IExcelService
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<ItContract> _itContractRepository;
        private readonly IGenericRepository<OrganizationRight> _orgRightRepository;
        private readonly IExcelHandler _excelHandler;
        private readonly ICryptoService _cryptoService;

        public ExcelService(IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<ItContract> itContractRepository,
            IGenericRepository<OrganizationRight> orgRightRepository,
            IExcelHandler excelHandler,
            ICryptoService cryptoService)
        {
            _orgUnitRepository = orgUnitRepository;
            _userRepository = userRepository;
            _itContractRepository = itContractRepository;
            _orgRightRepository = orgRightRepository;
            _excelHandler = excelHandler;
            _cryptoService = cryptoService;
        }

        /// <summary>
        /// Exports users.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="organizationId">The organization identifier.</param>
        /// <param name="kitosUSer">The kitos u ser.</param>
        /// <returns></returns>
        public Stream ExportUsers(Stream stream, int organizationId, User kitosUSer)
        {
            var users = _userRepository.Get(x => x.OrganizationRights.Count(r => r.OrganizationId == organizationId) > 0);

            var set = new DataSet();
            set.Tables.Add(GetUserTable(users));

            return _excelHandler.Export(set, stream);
        }

        /// <summary>
        /// Exports organization units.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="organizationId">The organization identifier.</param>
        /// <param name="kitosUser">The kitos user.</param>
        /// <returns></returns>
        public Stream ExportOrganizationUnits(Stream stream, int organizationId, User kitosUser)
        {
            var orgUnits = _orgUnitRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .ToList();

            var set = new DataSet();
            set.Tables.Add(GetOrganizationTable(orgUnits));

            return _excelHandler.Export(set, stream);
        }

        public void ImportUsers(Stream stream, int organizationId, User kitosUser)
        {
            var scope = new TransactionScope(TransactionScopeOption.RequiresNew);
            using (scope)
            {
                var errors = new List<ExcelImportError>();
                var set = _excelHandler.Import(stream);

                // importing org units
                var userTable = set.Tables[0];
                errors.AddRange(ImportUsersTransaction(userTable, organizationId, kitosUser));

                // then finally, did we notice any errors?
                if (errors.Any()) throw new ExcelImportException() { Errors = errors };

                // if we got here, we're home frreeeee
                scope.Complete();
            }
        }

        public void ExportItContracts(Stream stream, int organizationId, User kitosUser)
        {
            var contracts = _itContractRepository.Get(x => x.OrganizationId == organizationId);

            var set = new DataSet();
            set.Tables.Add(GetItContractTable(contracts));

            _excelHandler.Export(set, stream);
        }

        public void ImportItContracts(Stream stream, int organizationId, User kitosUser)
        {
            // read excel stream to DataSet
            var contractDataSet = _excelHandler.Import(stream);
            // get contracts table
            var contractDataTable = contractDataSet.Tables[0];
            // import contracts
            var errors = ImportItContractsTransaction(contractDataTable, organizationId, kitosUser).ToList();

            // then finally, did we notice any errors?
            if (errors.Any())
                throw new ExcelImportException { Errors = errors };
        }

        private IEnumerable<ExcelImportError> ImportItContractsTransaction(DataTable contractTable, int organizationId, User kitosUser)
        {
            var errors = new List<ExcelImportError>();

            // select only contracts that should be inserted
            var newContracts = contractTable.Select("[Column1] IS NULL OR [Column1] = ''").AsEnumerable().ToList();
            var firstRow = newContracts.FirstOrDefault();

            // if nothing to add then abort here
            if (firstRow == null)
            {
                errors.Add(new ExcelImportError { Message = "Intet at importere!"});
                return errors;
            }

            var rowIndex = contractTable.Rows.IndexOf(firstRow) + 2; // adding 2 to get it to lign up with row numbers in excel
            foreach (var row in newContracts)
            {
                var contractRow = new ContractRow
                {
                    RowIndex = rowIndex,
                    Name = row.Field<string>(1),
                    ItContractId = row.Field<string>(2),
                    Esdh = row.Field<string>(3),
                    Folder = row.Field<string>(4),
                    Note = row.Field<string>(5),
                    ConcludedText = row.Field<string>(6),
                    IrrevocableToText = row.Field<string>(7),
                    ExpirationDateText = row.Field<string>(8),
                    TerminatedText = row.Field<string>(9),
                };

                // validate that name exists
                if (String.IsNullOrEmpty(contractRow.Name) || String.IsNullOrWhiteSpace(contractRow.Name))
                {
                    var error = new ExcelImportError
                    {
                        Row = contractRow.RowIndex,
                        Column = "B",
                        Message = "IT Kontrakt Navn mangler",
                        SheetName = "IT Kontrakter"
                    };
                    errors.Add(error);
                }

                // validate Concluded is a date
                try
                {
                    // is something entered?
                    if (!String.IsNullOrEmpty(contractRow.ConcludedText) && !String.IsNullOrWhiteSpace(contractRow.ConcludedText))
                        contractRow.Concluded = DateTime.FromOADate(Double.Parse(contractRow.ConcludedText)).Date;
                }
                catch (Exception)
                {
                    var error = new ExcelImportError
                    {
                        Row = contractRow.RowIndex,
                        Column = "G",
                        Message = "Indgået er ikke en gyldig dato",
                        SheetName = "IT Kontrakter"
                    };
                    errors.Add(error);
                }

                // validate IrrevocableTo is a date
                try
                {
                    // is something entered?
                    if (!String.IsNullOrEmpty(contractRow.IrrevocableToText) && !String.IsNullOrWhiteSpace(contractRow.IrrevocableToText))
                        contractRow.IrrevocableTo = DateTime.FromOADate(Double.Parse(contractRow.IrrevocableToText)).Date;
                }
                catch (Exception)
                {
                    var error = new ExcelImportError
                    {
                        Row = contractRow.RowIndex,
                        Column = "H",
                        Message = "'Uopsigeligt til' er ikke en gyldig dato",
                        SheetName = "IT Kontrakter"
                    };
                    errors.Add(error);
                }

                // validate ExpirationDate is a date
                try
                {
                    // is something entered?
                    if (!String.IsNullOrEmpty(contractRow.ExpirationDateText) && !String.IsNullOrWhiteSpace(contractRow.ExpirationDateText))
                        contractRow.ExpirationDate = DateTime.FromOADate(Double.Parse(contractRow.ExpirationDateText)).Date;
                }
                catch (Exception)
                {
                    var error = new ExcelImportError
                    {
                        Row = contractRow.RowIndex,
                        Column = "I",
                        Message = "Udløbsdato er ikke en gyldig dato",
                        SheetName = "IT Kontrakter"
                    };
                    errors.Add(error);
                }

                // validate Terminated is a date
                try
                {
                    // is something entered?
                    if (!String.IsNullOrEmpty(contractRow.TerminatedText) && !String.IsNullOrWhiteSpace(contractRow.TerminatedText))
                        contractRow.Terminated = DateTime.FromOADate(Double.Parse(contractRow.TerminatedText)).Date;
                }
                catch (Exception)
                {
                    var error = new ExcelImportError
                    {
                        Row = contractRow.RowIndex,
                        Column = "J",
                        Message = "'Kontrakten opsagt' er ikke en gyldig dato",
                        SheetName = "IT Kontrakter"
                    };
                    errors.Add(error);
                }

                _itContractRepository.Insert(new ItContract
                {
                    Name = contractRow.Name,
                    ItContractId = contractRow.ItContractId,
                    Esdh = contractRow.Esdh,
                    Folder = contractRow.Folder,
                    Note = contractRow.Note,
                    Concluded = contractRow.Concluded,
                    IrrevocableTo = contractRow.IrrevocableTo,
                    ExpirationDate = contractRow.ExpirationDate,
                    Terminated = contractRow.Terminated,
                    OrganizationId = organizationId,
                });

                rowIndex++;
            }
            // no errors found, it's safe to save to DB
            if (!errors.Any())
                _itContractRepository.Save();

            return errors;
        }

        private IEnumerable<ExcelImportError> ImportUsersTransaction(DataTable userTable, int organizationId, User kitosUser)
        {
            var errors = new List<ExcelImportError>();

            var newUsers = userTable.Select("[Column1] IS NULL OR [Column1] = ''").AsEnumerable().ToList();
            var firstRow = newUsers.FirstOrDefault();

            // if nothing to add then abort here
            if (firstRow == null)
            {
                errors.Add(new ExcelImportError { Message = "Intet at importere!" });
                return errors;
            }

            // unresolved rows are orgUnits which still needs to be added to the DB.
            var unresolvedRows = new List<UserRow>();

            var rowIndex = userTable.Rows.IndexOf(firstRow) + 2; // adding 2 to get it to lign up with row numbers in excel

            // preliminary pass and error checking
            // and the new rows that the users has added to the sheet
            foreach (var row in newUsers)
            {
                // a row is new if the first column, the id, is empty
                var id = StringToId(row.Field<string>(0));
                var isNew = (id == null);

                var userRow = new UserRow()
                {
                    RowIndex = rowIndex, // needed for error reporting
                    IsNew = isNew,
                    Id = id,
                    Name = row.Field<string>(1),
                    LastName = row.Field<string>(2),
                    Email = row.Field<string>(3),
                    Phone = String.IsNullOrWhiteSpace(row.Field<string>(4)) ? null : row.Field<string>(4)
                };

                var foundError = false;

                // error checking
                // firstname cannot be empty
                if (String.IsNullOrWhiteSpace(userRow.Name))
                {
                    var error = new ExcelImportError
                    {
                        Row = userRow.RowIndex,
                        Column = "B",
                        Message = "Fornavn mangler",
                        SheetName = "Brugere"
                    };
                    errors.Add(error);
                    foundError = true;
                }
                // lastname cannot be empty
                if (String.IsNullOrWhiteSpace(userRow.LastName))
                {
                    var error = new ExcelImportError
                    {
                        Row = userRow.RowIndex,
                        Column = "C",
                        Message = "Efternavn(e) mangler",
                        SheetName = "Brugere"
                    };
                    errors.Add(error);
                    foundError = true;
                }
                // email cannot be empty
                if (String.IsNullOrWhiteSpace(userRow.Email))
                {
                    var error = new ExcelImportError
                    {
                        Row = userRow.RowIndex,
                        Column = "D",
                        Message = "Email mangler",
                        SheetName = "Brugere"
                    };
                    errors.Add(error);
                    foundError = true;
                }

                if (isNew && !foundError)
                {
                    unresolvedRows.Add(userRow);
                }

                rowIndex++;
            }

            // do the actually passes, trying to resolve parents
            var oneMorePass = true;
            while (oneMorePass && unresolvedRows.Any())
            {
                oneMorePass = false;

                var resolvedInThisPass = new List<UserRow>();

                foreach (var userRow in unresolvedRows)
                {
                    var userEntity = new User
                    {
                        Name = userRow.Name,
                        LastName = userRow.LastName,
                        Email = userRow.Email,
                        PhoneNumber = userRow.Phone,
                        IsGlobalAdmin = false,
                        Password = _cryptoService.Encrypt(Guid.NewGuid().ToString("N")),
                        Salt = _cryptoService.Encrypt(Guid.NewGuid().ToString("N"))
                    };

                    // if user dosnt exist create a new one.
                    if (!_userRepository.Get(x => x.Email == userEntity.Email).Any())
                    {
                        _userRepository.Insert(userEntity);
                        _userRepository.Save();
                    }
                    else
                    {
                        // fetch user to ensure we're using the correct information
                        userEntity = _userRepository.Get(x => x.Email == userEntity.Email).First();
                    }

                    resolvedInThisPass.Add(userRow);

                    // if adminRight exists, no further action is needed
                    if (_orgRightRepository.Get(x => x.User.Email == userEntity.Email && x.OrganizationId == organizationId).Any())
                        continue;

                    // create the adminright within the organization
                    _orgRightRepository.Insert(new OrganizationRight
                    {
                        OrganizationId = organizationId,
                        UserId = userEntity.Id,
                        Role = OrganizationRole.User,
                    });
                    _orgRightRepository.Save();
                }
            }
            return errors;
        }

        /// <summary>
        /// Imports organization units into an organization.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="organizationId">The organization identifier.</param>
        /// <param name="kitosUser">The kitos user.</param>
        /// <exception cref="ExcelImportException"></exception>
        public void ImportOrganizationUnits(Stream stream, int organizationId, User kitosUser)
        {
            var scope = new TransactionScope(TransactionScopeOption.RequiresNew);
            using (scope)
            {
                var errors = new List<ExcelImportError>();

                var set = _excelHandler.Import(stream);

                // importing org units
                var orgTable = set.Tables[0];
                errors.AddRange(ImportOrgUnits(orgTable, organizationId, kitosUser));

                // then finally, did we notice any errors?
                if (errors.Any()) throw new ExcelImportException() { Errors = errors };

                // if we got here, we're home frreeeee
                scope.Complete();
            }
        }

        private static long? StringToEan(string s)
        {
            if (String.IsNullOrEmpty(s)) return null;

            // if the ean was properly entered, excel will treat is as a Number,
            // which is bascially a string in double format i.e "12345678.0"
            // so try to parse as double first
            double dbl;
            if (!Double.TryParse(s, out dbl)) return null;

            //then convert to long
            return Convert.ToInt64(dbl);
        }

        private static int? StringToId(string s)
        {
            if (String.IsNullOrEmpty(s))
                return null;
            return Convert.ToInt32(Convert.ToDouble(s));
        }

        private IEnumerable<ExcelImportError> ImportOrgUnits(DataTable orgTable, int organizationId, User kitosUser)
        {
            var errors = new List<ExcelImportError>();

            // resolvedRows are the orgUnits that already has been added to the DB.
            // the key is the name of the orgUnit;
            var resolvedRows = new Dictionary<string, OrgUnitRow>();

            // unresolved rows are orgUnits which still needs to be added to the DB.
            var unresolvedRows = new List<OrgUnitRow>();

            // preliminary pass and error checking
            // split the rows into the old org units (already in db)
            // and the new rows that the users has added to the sheet
            var rowIndex = 2;
            foreach (var row in orgTable.AsEnumerable())
            {
                // a row is new if the first column, the id, is empty
                var id = StringToId(row.Field<string>(0));
                var isNew = (id == null);

                var orgUnitRow = new OrgUnitRow
                {
                    RowIndex = rowIndex, // needed for error reporting
                    IsNew = isNew,
                    Id = id,
                    Parent = row.Field<string>(1),
                    Name = row.Field<string>(2),
                    Ean = StringToEan(row.Field<string>(3))
                };

                rowIndex++;

                // error checking
                // name cannot be empty
                if (String.IsNullOrWhiteSpace(orgUnitRow.Name))
                {
                    errors.Add(new ExcelImportOrgUnitNoNameError(orgUnitRow.RowIndex));
                }
                // ean must be valid
                else if (isNew && !String.IsNullOrWhiteSpace(row.Field<string>(3)) && !(orgUnitRow.Ean.HasValue && orgUnitRow.Ean.ToString().Length == 13))
                {
                    var error = new ExcelImportError()
                    {
                        Row = orgUnitRow.RowIndex,
                        Column = "D",
                        Message = "EAN værdien er ikke gyldig",
                        SheetName = "Organisationsenheder"
                    };
                    errors.Add(error);
                }
                // name cannot be duplicate
                else if (unresolvedRows.Any(x => x.Name == orgUnitRow.Name) || resolvedRows.ContainsKey(orgUnitRow.Name))
                {
                    errors.Add(new ExcelImportOrgUnitDuplicateNameError(orgUnitRow.RowIndex));
                }
                else if (orgUnitRow.IsNew && orgUnitRow.Ean != null && _orgUnitRepository.AsQueryable().Any(x => x.Ean == orgUnitRow.Ean))
                {
                    errors.Add(new ExcelImportOrgUnitDuplicateEanError(orgUnitRow.RowIndex));
                }
                // parent cannot be empty on a new row
                else if (orgUnitRow.IsNew && String.IsNullOrWhiteSpace(orgUnitRow.Parent))
                {
                    errors.Add(new ExcelImportOrgUnitBadParentError(orgUnitRow.RowIndex));
                }

                // otherwise we're good - add the row to either resolved or unresolved
                else if (isNew)
                {
                    unresolvedRows.Add(orgUnitRow);
                }
                else
                {
                    resolvedRows.Add(orgUnitRow.Name, orgUnitRow);
                }

            }

            // do the actually passes, trying to resolve parents
            var oneMorePass = true;
            while (oneMorePass && unresolvedRows.Any())
            {
                oneMorePass = false;

                var notResolvedInThisPass = new List<OrgUnitRow>();
                var resolvedInThisPass = new List<OrgUnitRow>();

                foreach (var orgUnitRow in unresolvedRows)
                {
                    // try to locate a parent
                    if (resolvedRows.TryGetValue(orgUnitRow.Parent, out var parent))
                    {

                        // since a parent was found, insert the new org unit in the DB.
                        var orgUnitEntity = _orgUnitRepository.Insert(new OrganizationUnit
                        {
                            Name = orgUnitRow.Name,
                            Ean = orgUnitRow.Ean,
                            ParentId = parent.Id,
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
                    // save repository - so the ids of the newly added org units are resolved
                    _orgUnitRepository.Save();

                    foreach (var orgUnitRow in resolvedInThisPass)
                    {
                        orgUnitRow.Id = orgUnitRow.Proxy.Id;
                        resolvedRows.Add(orgUnitRow.Name, orgUnitRow);
                    }
                }

                // if there's still some unresolve rows left, try again
                unresolvedRows = notResolvedInThisPass;
            }

            // at this point, if there's is any unresolvedRows, we should report some errors
            errors.AddRange(unresolvedRows.Select(orgUnitRow => new ExcelImportOrgUnitBadParentError(orgUnitRow.RowIndex)));
            return errors;
        }

        public class ExcelImportOrgUnitBadParentError : ExcelImportError
        {
            public ExcelImportOrgUnitBadParentError(int row)
            {
                Row = row;
                Column = "B";
                SheetName = "Organisationsenheder";
                Message = "Overordnet enhed må ikke være blank og skal henvise til en gyldig enhed.";
            }
        }

        public class ExcelImportOrgUnitNoNameError : ExcelImportError
        {
            public ExcelImportOrgUnitNoNameError(int row)
            {
                Row = row;
                Column = "C";
                SheetName = "Organisationsenheder";
                Message = "Enheden skal have et navn.";
            }
        }

        public class ExcelImportOrgUnitDuplicateNameError : ExcelImportError
        {
            public ExcelImportOrgUnitDuplicateNameError(int row)
            {
                Row = row;
                Column = "C";
                SheetName = "Organisationsenheder";
                Message = "Der findes allerede en enhed med dette navn.";
            }
        }

        public class ExcelImportOrgUnitDuplicateEanError : ExcelImportError
        {
            public ExcelImportOrgUnitDuplicateEanError(int row)
            {
                Row = row;
                Column = "D";
                SheetName = "Organisationsenheder";
                Message = "Der findes allerede en enhed i KITOS med dette EAN nummer.";
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

        private class ContractRow
        {
            public int RowIndex { get; set; }
            public string Name { get; set; }
            public string ItContractId { get; set; }
            public string Esdh { get; set; }
            public string Folder { get; set; }
            public string Note { get; set; }
            public DateTime? Concluded { get; set; }
            public DateTime? IrrevocableTo { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public DateTime? Terminated { get; set; }
            public string ConcludedText { get; set; }
            public string IrrevocableToText { get; set; }
            public string ExpirationDateText { get; set; }
            public string TerminatedText { get; set; }
        }

        #region Table Helpers

        private static DataTable GetOrganizationTable(IEnumerable<OrganizationUnit> orgUnits)
        {
            var table = new DataTable("Organisationsenheder");
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();

            foreach (var orgUnit in orgUnits)
            {
                var parent = "";
                if (orgUnit.Parent != null)
                    parent = orgUnit.Parent.Name;

                table.Rows.Add(orgUnit.Id, parent, orgUnit.Name, orgUnit.Ean);
            }

            return table;
        }

        private static DataTable GetUserTable(IEnumerable<User> users)
        {
            var table = new DataTable("Brugere");
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();
            table.Columns.Add();

            foreach (var user in users)
                table.Rows.Add(user.Id, user.Name, user.LastName, user.Email, user.PhoneNumber);

            return table;
        }

        private static DataTable GetItContractTable(IEnumerable<ItContract> itContracts)
        {
            var table = new DataTable("IT Kontrakter");

            // add columns according to fields added below
            for (var i = 0; i < 10; i++)
                table.Columns.Add();

            foreach (var contract in itContracts)
            {
                var concluded = contract.Concluded.HasValue ? contract.Concluded.Value.ToShortDateString() : null;
                var irrevocableTo = contract.IrrevocableTo.HasValue ? contract.IrrevocableTo.Value.ToShortDateString() : null;
                var expirationDate = contract.ExpirationDate.HasValue ? contract.ExpirationDate.Value.ToShortDateString() : null;
                var terminated = contract.Terminated.HasValue ? contract.Terminated.Value.ToShortDateString() : null;
                table.Rows.Add(contract.Id, contract.Name, contract.ItContractId, contract.Esdh, contract.Folder, contract.Note, concluded, irrevocableTo, expirationDate, terminated);
            }

            return table;
        }

        #endregion
    }
}
