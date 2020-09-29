namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDataResponsible : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.DataProcessingRegistrations", name: "DataProcessingDataResponsibleOption_Id", newName: "DataResponsible_Id");
            RenameIndex(table: "dbo.DataProcessingRegistrations", name: "IX_DataProcessingDataResponsibleOption_Id", newName: "IX_DataResponsible_Id");
            AddColumn("dbo.DataProcessingRegistrations", "DataResponsibleRemark", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsible", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsibleRemark", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsibleRemark");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsible");
            DropColumn("dbo.DataProcessingRegistrations", "DataResponsibleRemark");
            RenameIndex(table: "dbo.DataProcessingRegistrations", name: "IX_DataResponsible_Id", newName: "IX_DataProcessingDataResponsibleOption_Id");
            RenameColumn(table: "dbo.DataProcessingRegistrations", name: "DataResponsible_Id", newName: "DataProcessingDataResponsibleOption_Id");
        }
    }
}
