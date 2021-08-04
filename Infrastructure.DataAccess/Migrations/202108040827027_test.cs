namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KendoColumnConfigurations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersistId = c.String(),
                        Index = c.Int(nullable: false),
                        Hidden = c.Boolean(nullable: false),
                        KendoOrganizationalConfigurationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KendoOrganizationalConfigurations", t => t.KendoOrganizationalConfigurationId, cascadeDelete: true)
                .Index(t => t.KendoOrganizationalConfigurationId);
            
            AlterColumn("dbo.KendoOrganizationalConfigurations", "Version", c => c.String(nullable: false));
            DropColumn("dbo.KendoOrganizationalConfigurations", "VisibleColumnsCsv");
        }
        
        public override void Down()
        {
            AddColumn("dbo.KendoOrganizationalConfigurations", "VisibleColumnsCsv", c => c.String());
            DropForeignKey("dbo.KendoColumnConfigurations", "KendoOrganizationalConfigurationId", "dbo.KendoOrganizationalConfigurations");
            DropIndex("dbo.KendoColumnConfigurations", new[] { "KendoOrganizationalConfigurationId" });
            AlterColumn("dbo.KendoOrganizationalConfigurations", "Version", c => c.Int(nullable: false));
            DropTable("dbo.KendoColumnConfigurations");
        }
    }
}
