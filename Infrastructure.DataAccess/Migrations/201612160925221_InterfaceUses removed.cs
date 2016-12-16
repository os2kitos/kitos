namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InterfaceUsesremoved : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItInterfaceUsage", new[] { "ItSystemId", "ItInterfaceId" }, "dbo.ItInterfaceUses");
            DropForeignKey("dbo.ItInterfaceUses", "ItInterfaceId", "dbo.ItInterface");
            DropForeignKey("dbo.ItInterfaceUses", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItInterfaceUsage", "ItInterfaceId", "dbo.ItInterface");
            DropIndex("dbo.ItInterfaceUses", new[] { "ItSystemId" });
            DropIndex("dbo.ItInterfaceUses", new[] { "ItInterfaceId" });
            DropIndex("dbo.ItInterfaceUsage", new[] { "ItSystemId", "ItInterfaceId" });
            CreateIndex("dbo.ItInterfaceUsage", "ItInterfaceId");
            AddForeignKey("dbo.ItInterfaceUsage", "ItInterfaceId", "dbo.ItInterface", "Id");
            DropTable("dbo.ItInterfaceUses");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ItInterfaceUses",
                c => new
                    {
                        ItSystemId = c.Int(nullable: false),
                        ItInterfaceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemId, t.ItInterfaceId });
            
            DropForeignKey("dbo.ItInterfaceUsage", "ItInterfaceId", "dbo.ItInterface");
            DropIndex("dbo.ItInterfaceUsage", new[] { "ItInterfaceId" });
            CreateIndex("dbo.ItInterfaceUsage", new[] { "ItSystemId", "ItInterfaceId" });
            CreateIndex("dbo.ItInterfaceUses", "ItInterfaceId");
            CreateIndex("dbo.ItInterfaceUses", "ItSystemId");
            AddForeignKey("dbo.ItInterfaceUsage", "ItInterfaceId", "dbo.ItInterface", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItInterfaceUses", "ItSystemId", "dbo.ItSystem", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItInterfaceUses", "ItInterfaceId", "dbo.ItInterface", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItInterfaceUsage", new[] { "ItSystemId", "ItInterfaceId" }, "dbo.ItInterfaceUses", new[] { "ItSystemId", "ItInterfaceId" });
        }
    }
}
