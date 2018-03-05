namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Nameforurlonitcontractadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContract", "DataHandlerAgreementUrlName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItContract", "DataHandlerAgreementUrlName");
        }
    }
}
