namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Dpa_References : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingAgreements", "ReferenceId", c => c.Int());
            AddColumn("dbo.ExternalReferences", "DataProcessingAgreement_Id", c => c.Int());
            CreateIndex("dbo.DataProcessingAgreements", "ReferenceId");
            CreateIndex("dbo.ExternalReferences", "DataProcessingAgreement_Id");
            AddForeignKey("dbo.ExternalReferences", "DataProcessingAgreement_Id", "dbo.DataProcessingAgreements", "Id", cascadeDelete: true);
            AddForeignKey("dbo.DataProcessingAgreements", "ReferenceId", "dbo.ExternalReferences", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingAgreements", "ReferenceId", "dbo.ExternalReferences");
            DropForeignKey("dbo.ExternalReferences", "DataProcessingAgreement_Id", "dbo.DataProcessingAgreements");
            DropIndex("dbo.ExternalReferences", new[] { "DataProcessingAgreement_Id" });
            DropIndex("dbo.DataProcessingAgreements", new[] { "ReferenceId" });
            DropColumn("dbo.ExternalReferences", "DataProcessingAgreement_Id");
            DropColumn("dbo.DataProcessingAgreements", "ReferenceId");
        }
    }
}
