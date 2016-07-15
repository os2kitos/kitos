namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class MigrateDataOrgType : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.Organization SET TypeId = 3 WHERE Type=0");
            Sql("UPDATE dbo.Organization SET TypeId = 1 WHERE Type=1");
            Sql("UPDATE dbo.Organization SET TypeId = 2 WHERE Type=2");

            AddForeignKey("dbo.Organization", "TypeId", "dbo.OrganizationTypes", "Id");
            DropColumn("dbo.Organization", "Type");
        }

        public override void Down()
        {
            AddColumn("dbo.Organization", "Type", c => c.Int());
            DropForeignKey("dbo.Organization", "TypeId", "dbo.OrganizationTypes");
        }
    }
}
