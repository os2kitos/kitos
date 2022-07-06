namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Rename_Criticality_fields_on_contract : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.ItContract", name: "CriticalityTypeId", newName: "CriticalityId");
            RenameIndex(table: "dbo.ItContract", name: "IX_CriticalityTypeId", newName: "IX_CriticalityId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.ItContract", name: "IX_CriticalityId", newName: "IX_CriticalityTypeId");
            RenameColumn(table: "dbo.ItContract", name: "CriticalityId", newName: "CriticalityTypeId");
        }
    }
}
