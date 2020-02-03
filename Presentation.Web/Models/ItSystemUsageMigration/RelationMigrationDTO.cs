namespace Presentation.Web.Models.ItSystemUsageMigration
{
    public class RelationMigrationDTO
    {
        public NamedEntityDTO sourceSystem { get; set; }

        public NamedEntityDTO targetSystem { get; set; }

        public NamedEntityDTO itInterface { get; set; }
        public bool itInterfaceToBeDeleted { get; set; }

        public NamedEntityDTO itContract { get; set; }
}
}