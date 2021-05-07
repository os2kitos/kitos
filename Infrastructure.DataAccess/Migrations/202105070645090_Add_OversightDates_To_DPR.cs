﻿namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Infrastructure.DataAccess.Tools;

    public partial class Add_OversightDates_To_DPR : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProcessingRegistrationOversightDates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OversightDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        OversightRemark = c.String(),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.ParentId);

            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migration_DPR_LatestOversightDate.sql"));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingRegistrationOversightDates", "ParentId", "dbo.DataProcessingRegistrations");
            DropIndex("dbo.DataProcessingRegistrationOversightDates", new[] { "ParentId" });
            DropTable("dbo.DataProcessingRegistrationOversightDates");
        }
    }
}
