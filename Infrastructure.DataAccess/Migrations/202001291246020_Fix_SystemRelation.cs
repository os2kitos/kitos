namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fix_SystemRelation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SystemRelations", "Reference_Id", "dbo.ExternalLinks");
            DropIndex("dbo.SystemRelations", new[] { "Reference_Id" });
            RenameColumn(table: "dbo.SystemRelations", name: "AssociatedContract_Id", newName: "AssociatedContractId");
            RenameColumn(table: "dbo.SystemRelations", name: "RelationInterface_Id", newName: "RelationInterfaceId");
            RenameColumn(table: "dbo.SystemRelations", name: "UsageFrequency_Id", newName: "UsageFrequencyId");
            RenameIndex(table: "dbo.SystemRelations", name: "IX_RelationInterface_Id", newName: "IX_RelationInterfaceId");
            RenameIndex(table: "dbo.SystemRelations", name: "IX_UsageFrequency_Id", newName: "IX_UsageFrequencyId");
            RenameIndex(table: "dbo.SystemRelations", name: "IX_AssociatedContract_Id", newName: "IX_AssociatedContractId");
            AlterColumn("dbo.SystemRelations", "Reference_Id", c => c.Int());
            CreateIndex("dbo.SystemRelations", "Reference_Id");
            AddForeignKey("dbo.SystemRelations", "Reference_Id", "dbo.ExternalLinks", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SystemRelations", "Reference_Id", "dbo.ExternalLinks");
            DropIndex("dbo.SystemRelations", new[] { "Reference_Id" });
            AlterColumn("dbo.SystemRelations", "Reference_Id", c => c.Int(nullable: false));
            RenameIndex(table: "dbo.SystemRelations", name: "IX_AssociatedContractId", newName: "IX_AssociatedContract_Id");
            RenameIndex(table: "dbo.SystemRelations", name: "IX_UsageFrequencyId", newName: "IX_UsageFrequency_Id");
            RenameIndex(table: "dbo.SystemRelations", name: "IX_RelationInterfaceId", newName: "IX_RelationInterface_Id");
            RenameColumn(table: "dbo.SystemRelations", name: "UsageFrequencyId", newName: "UsageFrequency_Id");
            RenameColumn(table: "dbo.SystemRelations", name: "RelationInterfaceId", newName: "RelationInterface_Id");
            RenameColumn(table: "dbo.SystemRelations", name: "AssociatedContractId", newName: "AssociatedContract_Id");
            CreateIndex("dbo.SystemRelations", "Reference_Id");
            AddForeignKey("dbo.SystemRelations", "Reference_Id", "dbo.ExternalLinks", "Id", cascadeDelete: true);
        }
    }
}
