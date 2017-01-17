namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Advis : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Advice", "CarbonCopyReceiverId", "dbo.ItContractRoles");
            DropForeignKey("dbo.Advice", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.Advice", "ReceiverId", "dbo.ItContractRoles");
            DropIndex("dbo.Advice", new[] { "ReceiverId" });
            DropIndex("dbo.Advice", new[] { "CarbonCopyReceiverId" });
            DropIndex("dbo.Advice", new[] { "ItContractId" });
            CreateTable(
                "dbo.AdviceSents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdviceSentDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        AdviceId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Advice", t => t.AdviceId, cascadeDelete: true)
                .Index(t => t.AdviceId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.AdviceUserRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        RecieverType = c.Int(nullable: false),
                        RecpientType = c.Int(nullable: false),
                        AdviceId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Advice", t => t.AdviceId, cascadeDelete: true)
                .Index(t => t.AdviceId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            AddColumn("dbo.Advice", "RelationId", c => c.Int());
            AddColumn("dbo.Advice", "Type", c => c.Int());
            AddColumn("dbo.Advice", "Scheduling", c => c.Int());
            AddColumn("dbo.Advice", "StopDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.Advice", "Body", c => c.String());
            AddColumn("dbo.Advice", "JobId", c => c.String());
            Sql("SELECT * FROM [dbo].[Advice]\r\n\r\nUPDATE [dbo].[Advice]\r\nSET Type = 1, RelationId = ItContractId, JobId = \'Advice: \' + CONVERT(varchar(10), Id);\r\n\r\nINSERT INTO [dbo].[AdviceUserRelations]\r\n(RecieverType, Name, RecpientType, LastChanged, LastChangedByUserId, AdviceId)\r\nSELECT 1, u.Email, 3, a.LastChanged, a.LastChangedByUserId, a.Id FROM [dbo].[User] u, [dbo].[Advice] a\r\nWHERE u.Id = a.ReceiverId\r\n\r\nINSERT INTO [dbo].[AdviceUserRelations]\r\n(RecieverType, Name, RecpientType, LastChanged, LastChangedByUserId, AdviceId)\r\nSELECT 1, u.Email, 2, a.LastChanged, a.LastChangedByUserId, a.Id FROM [dbo].[User] u, [dbo].[Advice] a\r\nWHERE u.Id = a.CarbonCopyReceiverId");
            DropColumn("dbo.Advice", "ItContractId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Advice", "ItContractId", c => c.Int(nullable: false));
            DropForeignKey("dbo.AdviceUserRelations", "AdviceId", "dbo.Advice");
            DropForeignKey("dbo.AdviceUserRelations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.AdviceUserRelations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.AdviceSents", "AdviceId", "dbo.Advice");
            DropForeignKey("dbo.AdviceSents", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.AdviceSents", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.AdviceUserRelations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "AdviceId" });
            DropIndex("dbo.AdviceSents", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AdviceSents", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AdviceSents", new[] { "AdviceId" });
            DropColumn("dbo.Advice", "JobId");
            DropColumn("dbo.Advice", "Body");
            DropColumn("dbo.Advice", "StopDate");
            DropColumn("dbo.Advice", "Scheduling");
            DropColumn("dbo.Advice", "Type");
            DropColumn("dbo.Advice", "RelationId");
            DropTable("dbo.AdviceUserRelations");
            DropTable("dbo.AdviceSents");
            CreateIndex("dbo.Advice", "ItContractId");
            CreateIndex("dbo.Advice", "CarbonCopyReceiverId");
            CreateIndex("dbo.Advice", "ReceiverId");
            AddForeignKey("dbo.Advice", "ReceiverId", "dbo.ItContractRoles", "Id");
            AddForeignKey("dbo.Advice", "ItContractId", "dbo.ItContract", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Advice", "CarbonCopyReceiverId", "dbo.ItContractRoles", "Id");
        }
    }
}
