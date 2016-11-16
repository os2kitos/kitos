namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Hangfirecreatedatabase : DbMigration
    {
        public override void Up()
        {
            Sql(@"CREATE DATABASE kitos_HangfireDB", true);
        }
        
        public override void Down()
        {
            Sql(@"DROP DATABASE kitos_HangfireDB", true);
        }
    }
}
