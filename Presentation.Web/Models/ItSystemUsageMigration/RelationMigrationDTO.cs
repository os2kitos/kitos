namespace Presentation.Web.Models.ItSystemUsageMigration
{
    public class RelationMigrationDTO
    {
        public NamedEntityDTO ToSystemUsage { get; set; }

        public NamedEntityDTO FromSystemUsage { get; set; }

        public string Description { get; set; }

        public NamedEntityDTO Interface { get; set; }

        public NamedEntityDTO Contract { get; set; }
}
}