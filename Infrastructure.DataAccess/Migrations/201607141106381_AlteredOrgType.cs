namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AlteredOrgType : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrganizationTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Category = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Organization", "TypeId", c => c.Int(nullable: false));
            CreateIndex("dbo.Organization", "TypeId");
        }

        public override void Down()
        {
            DropIndex("dbo.Organization", new[] { "TypeId" });
            DropColumn("dbo.Organization", "TypeId");
            DropTable("dbo.OrganizationTypes");
        }
    }
}
