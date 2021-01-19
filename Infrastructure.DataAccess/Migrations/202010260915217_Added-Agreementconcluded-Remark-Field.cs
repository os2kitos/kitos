namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAgreementconcludedRemarkField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "AgreementConcludedRemark", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrations", "AgreementConcludedRemark");
        }
    }
}
