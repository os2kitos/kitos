namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveditContractRemarkentity : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItContractRemarks", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContractRemarks", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContractRemarks", "Id", "dbo.ItContract");
            DropIndex("dbo.ItContractRemarks", new[] { "Id" });
            DropIndex("dbo.ItContractRemarks", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItContractRemarks", new[] { "LastChangedByUserId" });
            DropTable("dbo.ItContractRemarks");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ItContractRemarks",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Remark = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.ItContractRemarks", "LastChangedByUserId");
            CreateIndex("dbo.ItContractRemarks", "ObjectOwnerId");
            CreateIndex("dbo.ItContractRemarks", "Id");
            AddForeignKey("dbo.ItContractRemarks", "Id", "dbo.ItContract", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItContractRemarks", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ItContractRemarks", "LastChangedByUserId", "dbo.User", "Id");
        }
    }
}
