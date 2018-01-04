namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class attachedoptionsadded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttachedOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ObjectId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        OptionType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AttachedOptions");
        }
    }
}
