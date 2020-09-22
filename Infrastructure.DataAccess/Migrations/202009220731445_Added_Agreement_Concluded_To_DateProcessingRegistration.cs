namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Agreement_Concluded_To_DateProcessingRegistration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "IsAgreementConcluded", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrations", "AgreementConcludedAt", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrations", "AgreementConcludedAt");
            DropColumn("dbo.DataProcessingRegistrations", "IsAgreementConcluded");
        }
    }
}
