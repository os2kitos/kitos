using System;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateDpaOptionTypesTask : DatabaseTask
    {
        /// <summary>
        /// Adds the default option types which are used for testing.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public override bool Execute(KitosContext dbContext)
        {
            var globalAdmin = dbContext.GetGlobalAdmin();
            var ownerId = globalAdmin.Id;

            //Oversight options
            dbContext.DataProcessingOversightOptions.Add(BuildOption(new DataProcessingOversightOption(), "Egen kontrol", 0, ownerId));
            dbContext.DataProcessingOversightOptions.Add(BuildOption(new DataProcessingOversightOption(), "Ledelseserklæring", 1, ownerId));
            dbContext.DataProcessingOversightOptions.Add(BuildOption(new DataProcessingOversightOption(), "ISAE 3000", 2, ownerId));
            dbContext.DataProcessingOversightOptions.Add(BuildOption(new DataProcessingOversightOption(), "ISAE 3402 type 1", 3, ownerId));
            dbContext.DataProcessingOversightOptions.Add(BuildOption(new DataProcessingOversightOption(), "ISAE 3402 type 2", 4, ownerId));
            dbContext.DataProcessingOversightOptions.Add(BuildOption(new DataProcessingOversightOption(), "Skriftlig kontrol", 5, ownerId));
            dbContext.DataProcessingOversightOptions.Add(BuildOption(new DataProcessingOversightOption(), "Fysisk tilsyn", 6, ownerId));

            //Oversight options
            dbContext.DataProcessingDataResponsibleOptions.Add(BuildOption(new DataProcessingDataResponsibleOption(), "Leverandøren er databehandler", 0, ownerId, "(Det er vurderet at leverandøren behandler persondata på instruks fra kommunen og der skal indgås en databehandleraftale)"));
            dbContext.DataProcessingDataResponsibleOptions.Add(BuildOption(new DataProcessingDataResponsibleOption(), "Leverandøren behandler ikke personoplysninger", 1, ownerId, "(Og derfor skal der ikke indgås en databehandleraftale)"));
            dbContext.DataProcessingDataResponsibleOptions.Add(BuildOption(new DataProcessingDataResponsibleOption(), "Leverandøren er selvstændig dataansvarlig", 2, ownerId, "(Deres anvendelse af data er ikke noget vi har indflydelse på)"));
            dbContext.DataProcessingDataResponsibleOptions.Add(BuildOption(new DataProcessingDataResponsibleOption(), "Fællesdataansvar ", 3, ownerId, "(der skal typisk indgås en anden type aftale – fortrolighedserklæring eller…)"));
            dbContext.DataProcessingDataResponsibleOptions.Add(BuildOption(new DataProcessingDataResponsibleOption(), "Kommunen er selv dataansvarlig", 4, ownerId));

            //data transfer options
            dbContext.DataProcessingBasisForTransferOptions.Add(BuildOption(new DataProcessingBasisForTransferOption(), "EU og EØS", 0, ownerId));
            dbContext.DataProcessingBasisForTransferOptions.Add(BuildOption(new DataProcessingBasisForTransferOption(), "EU's standard kontrakt /standard contract clauses", 1, ownerId));
            dbContext.DataProcessingBasisForTransferOptions.Add(BuildOption(new DataProcessingBasisForTransferOption(), "Binding corporate rules / BCR", 2, ownerId));
            dbContext.DataProcessingBasisForTransferOptions.Add(BuildOption(new DataProcessingBasisForTransferOption(), "Intet", 3, ownerId));
            dbContext.DataProcessingBasisForTransferOptions.Add(BuildOption(new DataProcessingBasisForTransferOption(), "Andet", 4, ownerId));

            //cointries
            dbContext.DataProcessingCountryOptions.Add(BuildOption(new DataProcessingCountryOption(), "Danmark", 0, ownerId));
            dbContext.DataProcessingCountryOptions.Add(BuildOption(new DataProcessingCountryOption(), "EU", 1, ownerId));

            dbContext.SaveChanges();
            return true;
        }

        private static T BuildOption<T>(T option, string name, int optionPriority, int ownerId, string helpText = null) where T : OptionEntity<DataProcessingAgreement>, IOptionReference<DataProcessingAgreement>
        {
            option.Name = name;
            option.Priority = optionPriority;
            option.LastChangedByUserId = ownerId;
            option.ObjectOwnerId = ownerId;
            option.LastChanged = DateTime.UtcNow;
            option.Description = helpText;
            return option;
        }
    }
}
