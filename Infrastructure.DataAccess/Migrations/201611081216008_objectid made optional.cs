namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class objectidmadeoptional : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ExternalReferences", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ExternalReferences", "ItContract_Id", "dbo.ItContract");
            DropIndex("dbo.ExternalReferences", new[] { "ItProject_Id" });
            DropIndex("dbo.ExternalReferences", new[] { "ItContract_Id" });
            AlterColumn("dbo.ExternalReferences", "ItProject_Id", c => c.Int());
            CreateIndex("dbo.ExternalReferences", "ItProject_Id");
            CreateIndex("dbo.ExternalReferences", "Itcontract_Id");
            AddForeignKey("dbo.ExternalReferences", "ItSystem_Id", "dbo.ItSystem", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ExternalReferences", "Itcontract_Id", "dbo.ItContract", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExternalReferences", "Itcontract_Id", "dbo.ItContract");
            DropForeignKey("dbo.ExternalReferences", "ItSystem_Id", "dbo.ItSystem");
            DropIndex("dbo.ExternalReferences", new[] { "Itcontract_Id" });
            DropIndex("dbo.ExternalReferences", new[] { "ItProject_Id" });
            AlterColumn("dbo.ExternalReferences", "ItProject_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.ExternalReferences", "ItContract_Id");
            CreateIndex("dbo.ExternalReferences", "ItProject_Id");
            AddForeignKey("dbo.ExternalReferences", "ItContract_Id", "dbo.ItContract", "Id");
            AddForeignKey("dbo.ExternalReferences", "ItSystem_Id", "dbo.ItSystem", "Id");
        }
    }
}
