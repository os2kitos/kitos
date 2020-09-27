namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Data_Responsible_Options : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.DataProcessingRegistrations", name: "DataProcessingDataResponsibleOption_Id", newName: "DataResponsible_Id");
            RenameIndex(table: "dbo.DataProcessingRegistrations", name: "IX_DataProcessingDataResponsibleOption_Id", newName: "IX_DataResponsible_Id");
            AddColumn("dbo.DataProcessingRegistrations", "DataResponsibleRemark", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrations", "DataResponsibleRemark");
            RenameIndex(table: "dbo.DataProcessingRegistrations", name: "IX_DataResponsible_Id", newName: "IX_DataProcessingDataResponsibleOption_Id");
            RenameColumn(table: "dbo.DataProcessingRegistrations", name: "DataResponsible_Id", newName: "DataProcessingDataResponsibleOption_Id");
        }
    }
}
