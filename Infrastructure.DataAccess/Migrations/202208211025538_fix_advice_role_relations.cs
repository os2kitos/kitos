namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using Infrastructure.DataAccess.Tools;
    using System.Data.Entity.Migrations;

    public partial class fix_advice_role_relations : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.AdviceUserRelations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "LastChangedByUserId" });
            AddColumn("dbo.AdviceUserRelations", "ItContractRoleId", c => c.Int());
            AddColumn("dbo.AdviceUserRelations", "ItProjectRoleId", c => c.Int());
            AddColumn("dbo.AdviceUserRelations", "ItSystemRoleId", c => c.Int());
            AddColumn("dbo.AdviceUserRelations", "DataProcessingRegistrationRoleId", c => c.Int());
            AddColumn("dbo.AdviceUserRelations", "Email", c => c.String());

            //Migrate all names to the email column
            Sql(@"UPDATE dbo.AdviceUserRelations 
                  SET Email = Name;"
            );

            //Add missing owner and lastChanged to GlobalAdmin (1)
            Sql(@"UPDATE dbo.AdviceUserRelations 
                  SET ObjectOwnerId = 1 where ObjectOwnerId is null;"
            );
            Sql(@"UPDATE dbo.AdviceUserRelations 
                  SET LastChangedByUserId = 1 where LastChangedByUserId is null;"
            );

            AlterColumn("dbo.AdviceUserRelations", "ObjectOwnerId", c => c.Int(nullable: false));
            AlterColumn("dbo.AdviceUserRelations", "LastChangedByUserId", c => c.Int(nullable: false));
            CreateIndex("dbo.AdviceUserRelations", "ItContractRoleId");
            CreateIndex("dbo.AdviceUserRelations", "ItProjectRoleId");
            CreateIndex("dbo.AdviceUserRelations", "ItSystemRoleId");
            CreateIndex("dbo.AdviceUserRelations", "DataProcessingRegistrationRoleId");
            CreateIndex("dbo.AdviceUserRelations", "ObjectOwnerId");
            CreateIndex("dbo.AdviceUserRelations", "LastChangedByUserId");
            AddForeignKey("dbo.AdviceUserRelations", "DataProcessingRegistrationRoleId", "dbo.DataProcessingRegistrationRoles", "Id");
            AddForeignKey("dbo.AdviceUserRelations", "ItContractRoleId", "dbo.ItContractRoles", "Id");
            AddForeignKey("dbo.AdviceUserRelations", "ItProjectRoleId", "dbo.ItProjectRoles", "Id");
            AddForeignKey("dbo.AdviceUserRelations", "ItSystemRoleId", "dbo.ItSystemRoles", "Id");
            DropColumn("dbo.Advice", "ReceiverId");
            DropColumn("dbo.Advice", "CarbonCopyReceiverId");
            DropColumn("dbo.AdviceUserRelations", "Name");

            // Migrate to role FKs and cleanup orphans
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migrate_AdviceUserRelations_Role_Relationships.sql"));

        }

        public override void Down()
        {
            AddColumn("dbo.AdviceUserRelations", "Name", c => c.String());
            AddColumn("dbo.Advice", "CarbonCopyReceiverId", c => c.Int());
            AddColumn("dbo.Advice", "ReceiverId", c => c.Int());
            DropForeignKey("dbo.AdviceUserRelations", "ItSystemRoleId", "dbo.ItSystemRoles");
            DropForeignKey("dbo.AdviceUserRelations", "ItProjectRoleId", "dbo.ItProjectRoles");
            DropForeignKey("dbo.AdviceUserRelations", "ItContractRoleId", "dbo.ItContractRoles");
            DropForeignKey("dbo.AdviceUserRelations", "DataProcessingRegistrationRoleId", "dbo.DataProcessingRegistrationRoles");
            DropIndex("dbo.AdviceUserRelations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "DataProcessingRegistrationRoleId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "ItSystemRoleId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "ItProjectRoleId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "ItContractRoleId" });
            AlterColumn("dbo.AdviceUserRelations", "LastChangedByUserId", c => c.Int());
            AlterColumn("dbo.AdviceUserRelations", "ObjectOwnerId", c => c.Int());
            DropColumn("dbo.AdviceUserRelations", "Email");
            DropColumn("dbo.AdviceUserRelations", "DataProcessingRegistrationRoleId");
            DropColumn("dbo.AdviceUserRelations", "ItSystemRoleId");
            DropColumn("dbo.AdviceUserRelations", "ItProjectRoleId");
            DropColumn("dbo.AdviceUserRelations", "ItContractRoleId");
            CreateIndex("dbo.AdviceUserRelations", "LastChangedByUserId");
            CreateIndex("dbo.AdviceUserRelations", "ObjectOwnerId");
        }
    }
}
