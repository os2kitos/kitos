namespace Presentation.Web.Models.ItSystemUsageMigration
{
    public class RelationMigrationDTO
    {
        public NamedEntityDTO SourceSystem { get; set; }

        public NamedEntityDTO TargetSystem { get; set; }

        public NamedEntityDTO RelationInterface { get; set; }

        public NamedEntityDTO RelationContract { get; set; }
}
}