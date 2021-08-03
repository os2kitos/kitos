namespace Presentation.Web.Models.API.V1.ItSystemUsageMigration
{
    public class RelationMigrationDTO
    {
        public NamedEntityWithEnabledStatusDTO ToSystemUsage { get; set; }

        public NamedEntityWithEnabledStatusDTO FromSystemUsage { get; set; }

        public string Description { get; set; }

        public NamedEntityDTO Interface { get; set; }

        public NamedEntityDTO FrequencyType { get; set; }

        public NamedEntityDTO Contract { get; set; }
}
}