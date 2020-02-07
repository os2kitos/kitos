namespace Presentation.Web.Models.ItSystemUsageMigration
{
    public class RelationMigrationDTO
    {
        public NamedEntityDTO SourceSystem { get; set; }

        public NamedEntityDTO TargetSystem { get; set; }

        public NamedEntityDTO Interface { get; set; }

        public NamedEntityDTO Contract { get; set; }
}
}