﻿namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Infrastructure.DataAccess.Tools;

    public partial class Added_Advicetype_To_Advice : DbMigration
    {
        public override void Up()
        {
            SqlResource(SqlMigrationScriptRepository.GetResourceName("FixAdviceIsActive.sql"));
            AddColumn("dbo.Advice", "AdviceType", c => c.Int(nullable: false));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migrate_Advice_Scheduling_To_AdviceType.sql"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Advice", "AdviceType");
        }
    }
}
