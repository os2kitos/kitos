using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainServices.GDPR;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateDataProcessingAgreementTask : DatabaseTask
    {
        private readonly string _name;

        public CreateDataProcessingAgreementTask(string name)
        {
            _name = name;
        }

        public override bool Execute(KitosContext dbContext)
        {
            var commonOrg = dbContext.GetOrganization(TestOrganizations.CommonOrg);
            var globalAdmin = dbContext.GetGlobalAdmin();

            var agreement = new DataProcessingAgreement()
            {
                Name = _name,
                ObjectOwnerId = globalAdmin.Id,
                OrganizationId = commonOrg.Id,
                LastChangedByUserId = globalAdmin.Id
            };

            agreement = dbContext.DataProcessingAgreements.Add(agreement);
            dbContext.SaveChanges();

            var readModel = new DataProcessingAgreementReadModel();
            new DataProcessingAgreementReadModelUpdate(new GenericRepository<DataProcessingAgreementRoleAssignmentReadModel>(dbContext)).Apply(agreement, readModel);

            dbContext.DataProcessingAgreementReadModels.Add(readModel);
            dbContext.SaveChanges();

            return true;
        }
    }
}
