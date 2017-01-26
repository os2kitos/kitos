namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Hangfirecreatedatabase : DbMigration
    {
        public override void Up()
        {
            Sql(@"IF NOT EXISTS(SELECT name FROM master.dbo.sysdatabases WHERE name = N'kitos_HangfireDB') CREATE DATABASE kitos_HangfireDB", true);
        }
        
        public override void Down()
        {
            Sql(@"DROP DATABASE kitos_HangfireDB", true);
        }
    }
}
