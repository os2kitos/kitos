namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_Advice_User_Relation_To_Be_Robust : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.AdviceUserRelations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "LastChangedByUserId" });
            AddColumn("dbo.AdviceUserRelations", "ItContractRoleRoleId", c => c.Int());
            AddColumn("dbo.AdviceUserRelations", "ItProjectRoleRoleId", c => c.Int());
            AddColumn("dbo.AdviceUserRelations", "ItSystemRoleId", c => c.Int());
            AddColumn("dbo.AdviceUserRelations", "DataProcessingRegistrationRoleId", c => c.Int());
            AddColumn("dbo.AdviceUserRelations", "Email", c => c.String());
            AlterColumn("dbo.AdviceUserRelations", "ObjectOwnerId", c => c.Int(nullable: false));
            AlterColumn("dbo.AdviceUserRelations", "LastChangedByUserId", c => c.Int(nullable: false));
            CreateIndex("dbo.AdviceUserRelations", "ItContractRoleRoleId");
            CreateIndex("dbo.AdviceUserRelations", "ItProjectRoleRoleId");
            CreateIndex("dbo.AdviceUserRelations", "ItSystemRoleId");
            CreateIndex("dbo.AdviceUserRelations", "DataProcessingRegistrationRoleId");
            CreateIndex("dbo.AdviceUserRelations", "ObjectOwnerId");
            CreateIndex("dbo.AdviceUserRelations", "LastChangedByUserId");
            AddForeignKey("dbo.AdviceUserRelations", "DataProcessingRegistrationRoleId", "dbo.DataProcessingRegistrationRoles", "Id");
            AddForeignKey("dbo.AdviceUserRelations", "ItContractRoleRoleId", "dbo.ItContractRoles", "Id");
            AddForeignKey("dbo.AdviceUserRelations", "ItProjectRoleRoleId", "dbo.ItProjectRoles", "Id");
            AddForeignKey("dbo.AdviceUserRelations", "ItSystemRoleId", "dbo.ItSystemRoles", "Id");
            DropColumn("dbo.Advice", "ReceiverId");
            DropColumn("dbo.Advice", "CarbonCopyReceiverId");
            //TODO: Add script that migrates the old role by name references into role by id references and deletes orphans!!!
            DropColumn("dbo.AdviceUserRelations", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AdviceUserRelations", "Name", c => c.String());
            AddColumn("dbo.Advice", "CarbonCopyReceiverId", c => c.Int());
            AddColumn("dbo.Advice", "ReceiverId", c => c.Int());
            DropForeignKey("dbo.AdviceUserRelations", "ItSystemRoleId", "dbo.ItSystemRoles");
            DropForeignKey("dbo.AdviceUserRelations", "ItProjectRoleRoleId", "dbo.ItProjectRoles");
            DropForeignKey("dbo.AdviceUserRelations", "ItContractRoleRoleId", "dbo.ItContractRoles");
            DropForeignKey("dbo.AdviceUserRelations", "DataProcessingRegistrationRoleId", "dbo.DataProcessingRegistrationRoles");
            DropIndex("dbo.AdviceUserRelations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "DataProcessingRegistrationRoleId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "ItSystemRoleId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "ItProjectRoleRoleId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "ItContractRoleRoleId" });
            AlterColumn("dbo.AdviceUserRelations", "LastChangedByUserId", c => c.Int());
            AlterColumn("dbo.AdviceUserRelations", "ObjectOwnerId", c => c.Int());
            DropColumn("dbo.AdviceUserRelations", "Email");
            DropColumn("dbo.AdviceUserRelations", "DataProcessingRegistrationRoleId");
            DropColumn("dbo.AdviceUserRelations", "ItSystemRoleId");
            DropColumn("dbo.AdviceUserRelations", "ItProjectRoleRoleId");
            DropColumn("dbo.AdviceUserRelations", "ItContractRoleRoleId");
            CreateIndex("dbo.AdviceUserRelations", "LastChangedByUserId");
            CreateIndex("dbo.AdviceUserRelations", "ObjectOwnerId");
        }
    }
}
