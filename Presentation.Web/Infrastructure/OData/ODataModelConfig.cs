using Core.DomainModel;
using Core.DomainModel.Archive;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Core.DomainModel.Qa.References;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Presentation.Web.Infrastructure.OData
{
    public static class ODataModelConfig
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            RegisterEntitySet<User>(builder, "Users");
            RegisterEntitySet<Organization>(builder, "Organizations");
            RegisterEntitySet<OrganizationUnit>(builder, "OrganizationUnits");
            RegisterEntitySet<OrganizationRight>(builder, "OrganizationRights");
            RegisterEntitySet<ItSystem>(builder, "ItSystems");
            RegisterEntitySet<ItSystemUsage>(builder, "ItSystemUsages");
            RegisterEntitySet<ItSystemRight>(builder, "ItSystemRights");
            RegisterEntitySet<ItInterface>(builder, "ItInterfaces");
            RegisterEntitySet<ItContract>(builder, "ItContracts");
            RegisterEntitySet<ItSystemUsageArchive>(builder, "ItSystemUsageArchives");
            RegisterEntitySet<ItSystemUsageOverviewReadModel>(builder, "ItSystemUsageOverviewReadModels");
            RegisterEntitySet<ItContractOverviewReadModel>(builder, "ItContractOverviewReadModels");
            RegisterEntitySet<ItContractSupplierOverviewReadModel>(builder, "ItContractSupplierOverviewReadModels");
            RegisterEntitySet<DataProcessingRegistrationReadModel>(builder, "DataProcessingRegistrationReadModels");

            // ExternalReference is a navigation target on ItSystem, ItSystemUsage, ItContract etc.
            // Register as entity type (no entity set needed) so $expand=Reference works.
            // Ignore navigations to unregistered types to prevent EDM build failures.
            var extRef = builder.EntityType<ExternalReference>();
            extRef.HasKey(e => e.Id);
            extRef.Ignore(e => e.BrokenLinkReports);
            extRef.Ignore(e => e.DataProcessingRegistration);

            // Register unbound functions so OData middleware sets up the correct entity-type context
            // before routing. Without this, [EnableQuery] has no EDM metadata and falls back to
            // full object-graph serialization, triggering EF6 lazy-loading explosion.
            var getUsersByUuid = builder.Function("GetUsersByUuid");
            getUsersByUuid.Parameter<System.Guid>("organizationUuid");
            getUsersByUuid.ReturnsCollectionFromEntitySet<User>("Users");

            return builder.GetEdmModel();
        }

        private static void RegisterEntitySet<T>(ODataConventionModelBuilder builder, string setName)
            where T : class, IHasId
        {
            builder.EntityType<T>().HasKey(e => e.Id);
            builder.EntitySet<T>(setName);
        }
    }
}
