namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedReferencesforITSystems : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "ReferenceId", c => c.Int());
            AddColumn("dbo.ExternalReferences", "ItSystem_Id", c => c.Int());
            CreateIndex("dbo.ItSystem", "ReferenceId");
            CreateIndex("dbo.ExternalReferences", "ItSystem_Id");
            AddForeignKey("dbo.ExternalReferences", "ItSystem_Id", "dbo.ItSystem", "Id");
            AddForeignKey("dbo.ItSystem", "ReferenceId", "dbo.ExternalReferences", "Id");
            Sql("INSERT INTO [dbo].ExternalReferences \r\n(ItSystem_Id, ExternalReferenceId, ObjectOwnerId, Title, Display, LastChanged, Created)\r\n(SELECT Id, Url, ObjectOwnerId, Title = \'Importeret reference\', Display = 0, LastChanged = GETDATE(), Created = LastChanged FROM [dbo].[ItSystem]\r\nWHERE Url NOT LIKE \'%http%\' AND Url is NOT null AND Url != \'\')");
            Sql("INSERT INTO [dbo].ExternalReferences \r\n(ItSystem_Id, ExternalReferenceId, ObjectOwnerId, Title, Display, LastChanged, Created)\r\n(SELECT Id, Url, ObjectOwnerId, Title = \'Importeret reference\', Display = 1, LastChanged = GETDATE(), Created = LastChanged FROM [dbo].[ItSystem]\r\nWHERE Url LIKE \'%http%\' AND Url is NOT null AND Url != \'\')");
            /*** Changed [AccessModifier] to public ('1') ***/
            Sql("UPDATE [dbo].[Organization]\r\nSET [AccessModifier] = \'1\'");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystem", "ReferenceId", "dbo.ExternalReferences");
            DropForeignKey("dbo.ExternalReferences", "ItSystem_Id", "dbo.ItSystem");
            DropIndex("dbo.ExternalReferences", new[] { "ItSystem_Id" });
            DropIndex("dbo.ItSystem", new[] { "ReferenceId" });
            DropColumn("dbo.ExternalReferences", "ItSystem_Id");
            DropColumn("dbo.ItSystem", "ReferenceId");
        }
    }
}
