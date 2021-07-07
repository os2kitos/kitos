namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Uuid_To_ItContract : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContract", "Uuid", c => c.Guid(nullable: false));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_Contract.sql"));
            CreateIndex("dbo.ItContract", "Uuid", unique: true, name: "UX_Contract_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItContract", "UX_Contract_Uuid");
            DropColumn("dbo.ItContract", "Uuid");
        }
    }
}
