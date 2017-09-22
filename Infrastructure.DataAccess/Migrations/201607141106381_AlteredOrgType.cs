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

            Sql("UPDATE dbo.Organization SET TypeId = 3 WHERE Type=0");
            Sql("UPDATE dbo.Organization SET TypeId = 1 WHERE Type=1");
            Sql("UPDATE dbo.Organization SET TypeId = 2 WHERE Type=2");

            // manually seed OrganizationTypes else the below AddForeignKey will fail
            Sql("INSERT INTO dbo.OrganizationTypes (Name, Category) VALUES ('Kommune', 1), ('Interessefællesskab', 1), ('Virksomhed', 0), ('Anden offentlig myndighed', 0);");

            AddForeignKey("dbo.Organization", "TypeId", "dbo.OrganizationTypes", "Id");
            DropColumn("dbo.Organization", "Type");
        }

        public override void Down()
        {
            AddColumn("dbo.Organization", "Type", c => c.Int());
            DropForeignKey("dbo.Organization", "TypeId", "dbo.OrganizationTypes");
            DropIndex("dbo.Organization", new[] { "TypeId" });
            DropColumn("dbo.Organization", "TypeId");
            DropTable("dbo.OrganizationTypes");
        }
    }
}
