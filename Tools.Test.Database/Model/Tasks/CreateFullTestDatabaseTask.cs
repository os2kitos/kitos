using System;
using System.Collections.Generic;
using Core.DomainModel.Organization;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Parameters;

namespace Tools.Test.Database.Model.Tasks
{
    /// <summary>
    /// Performs setup of a test database on the same connection as opposed to individual calls to the different CLI tasks.
    /// </summary>
    public class CreateFullTestDatabaseTask : DatabaseTask
    {
        private const string SecondOrganizationName = "Test organisation to";
        private const string DefaultOrganizationName = "Fælles Kommune";
        private readonly Credentials _globalAdmin;
        private readonly Credentials _localAdmin;
        private readonly Credentials _normalUser;
        private readonly Credentials _apiUser;
        private readonly Credentials _globalAdminApiUser;
        private readonly Credentials _systemIntegratorUser;
        private readonly IReadOnlyList<DatabaseTask> _tasks;

        public CreateFullTestDatabaseTask(
            Credentials globalAdmin,
            Credentials localAdmin,
            Credentials normalUser,
            Credentials apiUser,
            Credentials globalAdminApiUser,
            Credentials systemIntegratorUser)
        {
            _globalAdmin = globalAdmin;
            _localAdmin = localAdmin;
            _normalUser = normalUser;
            _apiUser = apiUser;
            _globalAdminApiUser = globalAdminApiUser;
            _systemIntegratorUser = systemIntegratorUser;

            _tasks = new List<DatabaseTask>
            {
                new CreateSensitivePersonalDataTypeTask("TestSensitiveData1"),
                new CreateSensitivePersonalDataTypeTask("TestSensitiveData2"),
                new CreateDprOptionTypesTask(),
                new EnableAllOptionsTask(),
                new CreateOrganizationTask((int)OrganizationTypeKeys.Kommune,SecondOrganizationName),
                new CreateKitosUserTask(_globalAdmin, OrganizationRole.GlobalAdmin, DefaultOrganizationName),
                new CreateKitosUserTask(_localAdmin, OrganizationRole.LocalAdmin, DefaultOrganizationName),
                new CreateKitosUserTask(_normalUser, OrganizationRole.User, DefaultOrganizationName),
                new CreateKitosUserTask(_apiUser, OrganizationRole.User, DefaultOrganizationName,true),
                new CreateKitosUserTask(_globalAdminApiUser, OrganizationRole.GlobalAdmin, $"{DefaultOrganizationName},{SecondOrganizationName}",true),
                new CreateKitosUserTask(_systemIntegratorUser, OrganizationRole.User, DefaultOrganizationName, true, true),
                new CreateItSystemTask("DefaultTestItSystem",DefaultOrganizationName,null),
                new CreateItSystemTask("SecondOrganizationDefaultTestItSystem",SecondOrganizationName,1),
                new CreateItInterfaceTask("DefaultItInterface", DefaultOrganizationName),
                new CreateItContractTask("DefaultTestItContract"),
                new CreateDataProcessingRegistrationTask("DefaultDpa"),
                new CreateTaskRefTask(DefaultOrganizationName)
            }.AsReadOnly();
        }

        public override bool Execute(KitosContext dbContext)
        {
            foreach (var databaseTask in _tasks)
            {
                Console.WriteLine($"Executing: {databaseTask}");
                databaseTask.Execute(dbContext);
                Console.WriteLine($"FINISHED: {databaseTask}");
            }

            return true;
        }
    }
}
