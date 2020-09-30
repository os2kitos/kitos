using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.LocalOptions;
using Core.DomainServices.GDPR;
using Core.DomainServices.Options;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateDataProcessingRegistrationTask : DatabaseTask
    {
        private readonly string _name;

        public CreateDataProcessingRegistrationTask(string name)
        {
            _name = name;
        }

        public override bool Execute(KitosContext dbContext)
        {
            var commonOrg = dbContext.GetOrganization(TestOrganizations.CommonOrg);
            var globalAdmin = dbContext.GetGlobalAdmin();

            var agreement = new DataProcessingRegistration()
            {
                Name = _name,
                ObjectOwnerId = globalAdmin.Id,
                OrganizationId = commonOrg.Id,
                LastChangedByUserId = globalAdmin.Id
            };

            agreement = dbContext.DataProcessingRegistrations.Add(agreement);
            dbContext.SaveChanges();

            var readModel = new DataProcessingRegistrationReadModel();
            var update = new DataProcessingRegistrationReadModelUpdate(
                new GenericRepository<DataProcessingRegistrationRoleAssignmentReadModel>(dbContext),
                new OptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption, LocalDataProcessingDataResponsibleOption>
                (
                    new GenericRepository<LocalDataProcessingDataResponsibleOption>(dbContext),
                    new GenericRepository<DataProcessingDataResponsibleOption>(dbContext)
                )
            );

            update.Apply(agreement, readModel);

            dbContext.DataProcessingRegistrationReadModels.Add(readModel);
            dbContext.SaveChanges();

            return true;
        }
    }
}
