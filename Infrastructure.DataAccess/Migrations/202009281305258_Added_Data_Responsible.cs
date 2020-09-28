namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Data_Responsible : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.DataProcessingRegistrations", name: "DataProcessingDataResponsibleOption_Id", newName: "DataResponsible_Id");
            RenameIndex(table: "dbo.DataProcessingRegistrations", name: "IX_DataProcessingDataResponsibleOption_Id", newName: "IX_DataResponsible_Id");
            AddColumn("dbo.DataProcessingRegistrations", "DataResponsibleRemark", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsibleRemark", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsible_Id", c => c.Int());
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "DataResponsible_Id");
            AddForeignKey("dbo.DataProcessingRegistrationReadModels", "DataResponsible_Id", "dbo.DataProcessingDataResponsibleOptions", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingRegistrationReadModels", "DataResponsible_Id", "dbo.DataProcessingDataResponsibleOptions");
            DropIndex("dbo.DataProcessingRegistrationReadModels", new[] { "DataResponsible_Id" });
            DropColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsible_Id");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsibleRemark");
            DropColumn("dbo.DataProcessingRegistrations", "DataResponsibleRemark");
            RenameIndex(table: "dbo.DataProcessingRegistrations", name: "IX_DataResponsible_Id", newName: "IX_DataProcessingDataResponsibleOption_Id");
            RenameColumn(table: "dbo.DataProcessingRegistrations", name: "DataResponsible_Id", newName: "DataProcessingDataResponsibleOption_Id");
        }
    }
}
