namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedlocaladminoptiontotoggleDataProcessorAgreementoverview : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Config", "ShowDataProcessorAgreement", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Config", "ShowDataProcessorAgreement");
        }
    }
}
